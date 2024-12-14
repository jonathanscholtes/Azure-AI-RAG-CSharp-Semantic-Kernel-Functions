using Azure.Search.Documents;
using Azure.Search.Documents.Models;

namespace ChatAPI.Data
{
    /// <summary>
    /// Provides methods for interacting with Azure Cognitive Search to retrieve documentation.
    /// </summary>
    public sealed class AISearchData
    {
        private readonly SearchClient _searchClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AISearchData"/> class.
        /// </summary>
        /// <param name="searchClient">The Azure SearchClient instance to perform search operations.</param>
        public AISearchData(SearchClient searchClient)
        {
            _searchClient = searchClient ?? throw new ArgumentNullException(nameof(searchClient));
        }

        /// <summary>
        /// Retrieves relevant documentation based on a user's query and a vectorized embedding.
        /// </summary>
        /// <param name="question">The search query provided by the user.</param>
        /// <param name="embedding">The vectorized representation of the user's query.</param>
        /// <returns>A list of dictionaries containing document metadata and content.</returns>
        public async Task<List<Dictionary<string, string?>>> RetrieveDocumentationAsync(string question, ReadOnlyMemory<float> embedding)
        {
            // Define search options with vector search configuration
            var searchOptions = new SearchOptions
            {
                VectorSearch = new()
                {
                    Queries =
                    {
                        new VectorizedQuery(embedding)
                        {
                            KNearestNeighborsCount = 3, // Return the 3 nearest documents
                            Fields = { "contentVector" } // Field containing the document vectors
                        }
                    }
                },
                Size = 3, // Limit results to 3 documents
                Select = { "reference_code", "title", "product_id", "content" }, // Fields to retrieve from the documents
                QueryType = SearchQueryType.Semantic, // Enable semantic search
                SemanticSearch = new SemanticSearchOptions
                {
                    SemanticConfigurationName = "default", // Name of the semantic configuration
                    QueryCaption = new QueryCaption(QueryCaptionType.Extractive), // Enable extractive captions
                    QueryAnswer = new QueryAnswer(QueryAnswerType.Extractive) // Enable extractive answers
                }
            };

            // Execute the search query
            var results = await _searchClient.SearchAsync<SearchDocument>(question, searchOptions);

            // Process the search results into a list of dictionaries
            var docs = new List<Dictionary<string, string?>>();
            await foreach (var doc in results.Value.GetResultsAsync())
            {
                docs.Add(new Dictionary<string, string?>
                {
                    { "reference_code", doc.Document["reference_code"]?.ToString() },
                    { "title", doc.Document["title"]?.ToString() },
                     { "product_id", doc.Document["product_id"]?.ToString() },
                    { "content", doc.Document["content"]?.ToString() }
                });
            }

            return docs;
        }
    }
}
