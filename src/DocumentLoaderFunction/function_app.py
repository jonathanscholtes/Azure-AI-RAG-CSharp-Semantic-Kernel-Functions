from os import environ
import json
from typing import List, Optional
import logging
import traceback
from bs4 import BeautifulSoup
import azure.functions as func
from azure.identity import DefaultAzureCredential
from azure.core.credentials import AzureKeyCredential
from azure.search.documents import SearchClient
from azure.search.documents.indexes import SearchIndexClient
from azure.search.documents.indexes.models import (
    SearchIndex,
    SimpleField,
    SearchableField,
    SearchField,
    SemanticConfiguration,
    SemanticField,
    VectorSearch,
    VectorSearchProfile,
    SemanticPrioritizedFields,
    HnswAlgorithmConfiguration,
    SemanticSearch
)
from azure.core.exceptions import ResourceNotFoundError
from langchain_openai import AzureOpenAIEmbeddings


app = func.FunctionApp(http_auth_level=func.AuthLevel.ANONYMOUS)


@app.blob_trigger(arg_name="myblob", path="load", connection="BlobTriggerConnection")
def Loader(myblob: func.InputStream):
    logging.info(f"Python blob trigger function processed blob\n"
                 f"Name: {myblob.name}\n"
                 f"Blob Size: {myblob.length} bytes")
    
    # Read the blob content
    blob_content = myblob.read()

    
    # Parse the blob content as JSON
    try:
        # Get the Azure Credential
        credential = DefaultAzureCredential()

        # Set the API type to `azure_ad`
        environ["OPENAI_API_TYPE"] = "azure_ad"
        # Set the API_KEY to the token from the Azure credential
        token = credential.get_token("https://cognitiveservices.azure.com/.default").token
        environ["OPENAI_API_KEY"] = token

        environ["AZURE_OPENAI_AD_TOKEN"] = environ["OPENAI_API_KEY"]

        # Configuration for Azure Cognitive Search
        search_endpoint = environ["AZURE_AI_SEARCH_ENDPOINT"]
        #search_api_key = environ["AZURE_AI_SEARCH_API_KEY"]
        index_name = environ["AZURE_AI_SEARCH_INDEX"]

        #AzureKeyCredential(token)

        # Create SearchClient
        search_client = SearchClient(endpoint=search_endpoint, index_name=index_name, credential=credential)

        # Create SearchIndexClient
        search_index_client = SearchIndexClient(endpoint=search_endpoint, credential=credential)

        json_data, text_data = html_to_json(blob_content)
        #data = json.loads(blob_content)
        logging.info(f"Blob content as JSON: {json_data}")

        
        logging.info(f"Create embeddings")
        embeddings: AzureOpenAIEmbeddings = AzureOpenAIEmbeddings(
        azure_deployment=environ.get("AZURE_OPENAI_EMBEDDING"),
        openai_api_version=environ.get("AZURE_OPENAI_API_VERSION"),
        azure_endpoint=environ.get("AZURE_OPENAI_ENDPOINT"),
        api_key=environ.get("AZURE_OPENAI_API_KEY"),)

        AISearchIndexLoader(embeddings,search_client,search_index_client,index_name,logging).populate_search_index(json_data,text_data)

    except json.JSONDecodeError as e:
        logging.error(f"Failed to parse blob content as JSON: {e}")
        logging.error(traceback.format_exc())
    except Exception as e:
        logging.error(f"loader Failed: {e}")
        logging.error(traceback.format_exc())

    

    
class AISearchIndexLoader:
    def __init__(self, embeddings, search_client, search_index_client, index_name,logging):
        self.logger = logging
        self.embeddings = embeddings
        self.search_client = search_client
        self.search_index_client = search_index_client
        self.index_name = index_name
    
    def populate_search_index(self,json_data,text_data):
        index_exists = False

        # Check if the index exists and contains documents
        try:
            self.logger.info("Verifying if AI Search index exists...")
            index_response = self.search_index_client.get_index(self.index_name)
            index_exists = True

            #search_results = self.search_client.search(search_text="*", top=1, include_total_count=True)
            #if search_results.get_count() > 0:
            #    self.logger.info("AI Search index already exists and contains documents. Nothing to do.")
            #   return
        except ResourceNotFoundError:
            self.logger.info("AI Search index not found, creating index...")

        # Create the index if it doesn't exist
        #semantic_settings=SemanticConfiguration(
                #            name="default",
                #            prioritized_fields=SemanticPrioritizedFields(
                #                title_field=SemanticField(field_name="title"),
                #                content_fields=[SemanticField(field_name="content")]
                #            )                        
                #),
        if not index_exists:

            semantic_config =SemanticConfiguration(
                           name="default",
                            prioritized_fields=SemanticPrioritizedFields(
                               title_field=SemanticField(field_name="title"),
                               content_fields=[SemanticField(field_name="content")]
                           ))

            index = SearchIndex(
                name=self.index_name,
                fields=[
                    SimpleField(name="reference_code", type="Edm.String", key=True, filterable=True, sortable=True),
                    SearchableField(name="content", type="Edm.String", filterable=True, sortable=True),
                    SearchableField(name="title", type="Edm.String", filterable=True, sortable=True),
                    SearchableField(name="product_id", type="Edm.Int", filterable=True, sortable=True),
                    SearchField(name="contentVector", type="Collection(Edm.Single)", vector_search_dimensions=1536, vector_search_profile_name="my-vector-config")
                ],
                
                semantic_search= SemanticSearch(configurations=[semantic_config]),
               
                vector_search = VectorSearch(
                        profiles=[VectorSearchProfile(name="my-vector-config", algorithm_configuration_name="my-algorithms-config")],
                        algorithms=[HnswAlgorithmConfiguration(name="my-algorithms-config", kind="hnsw")],
                    )

            )
            self.search_index_client.create_index(index)

       
        try:
            # Generate documents with embeddings
            documents = []
            ref_code = json_data["reference_code"]
            content = text_data
            title = json_data["title"]
            product_id = json_data["product_id"]
            embedding =  self.embeddings.embed_query(content)

            documents.append({
                "reference_code": ref_code,
                "content": content,
                "title": title,
                "product_id": product_id,
                "contentVector": embedding
            })

            result = self.search_client.upload_documents(documents=documents)

            self.logger.info("Upload of new document succeeded: {}".format(result[0].succeeded))


        except Exception as ex:
            self.logger.error("Error in AI Search: %s", ex)



def html_to_json(html_content):
    """
    Converts the given HTML content into a structured JSON format.
    
    :param html_content: The HTML content to be converted.
    :return: A dictionary representing the structured content in JSON.
    """
    soup = BeautifulSoup(html_content, "html.parser")
    
    data = {}

    # Extract title
    title = soup.find("title").text if soup.find("title") else "No Title"
    data["title"] = title

    # Extract main heading (h1)
    main_heading = soup.find("h1").text if soup.find("h1") else "No main heading"
    data["main_heading"] = main_heading

    # Extract description
    description_section = soup.find("h2", text="Description:")
    description_paragraphs = []
    if description_section:
        description_paragraphs = [p.text.strip() for p in description_section.find_all_next("p")]
    data["description"] = description_paragraphs

    # Extract possible error messages
    error_messages_section = soup.find("h2", text="Possible Error Messages:")
    error_messages = []
    if error_messages_section:
        error_messages = [li.text.strip() for li in error_messages_section.find_all_next("li")]
    data["possible_error_messages"] = error_messages

    # Extract resolution steps
    resolution_steps_section = soup.find("h2", text="Resolution Steps:")
    resolution_steps = []
    if resolution_steps_section:
        resolution_steps = [li.text.strip() for li in resolution_steps_section.find_all_next("li")]
    data["resolution_steps"] = resolution_steps

    # Extract next steps
    next_steps_section = soup.find("h2", text="Next Steps:")
    next_steps = []
    if next_steps_section:
        next_steps = [p.text.strip() for p in next_steps_section.find_all_next("p")]
    data["next_steps"] = next_steps

    # Extract reference code
    reference_code_section = soup.find("h2", text="Reference Code:")
    reference_code = reference_code_section.find_next("p").text.strip() if reference_code_section else "No reference code"
    data["reference_code"] = reference_code

     # Extract Product ID code
    product_id_section = soup.find("h2", text="Product ID:")
    product_id = product_id_section.find_next("p").text.strip() if product_id_section else "No product_id"
    data["product_id"] = product_id

     # Convert data to plain text format
    text_data = f"Title: {title}\n"
    text_data += f"Main Heading: {main_heading}\n\n"
    text_data += "Description:\n" + "\n".join(description_paragraphs) + "\n\n"
    text_data += "Possible Error Messages:\n" + "\n".join(error_messages) + "\n\n"
    text_data += "Resolution Steps:\n" + "\n".join(resolution_steps) + "\n\n"
    text_data += "Next Steps:\n" + "\n".join(next_steps) + "\n\n"
    text_data += f"Reference Code: {reference_code}\n"

    return data, text_data



