import { React, Component } from 'react'
import './Main.css'
import { Box, Typography, Button, Stack } from '@mui/material';
import AzureFreeAccount from './AzureFreeAccount';
import AzureServiceCards from './AzureServiceCards';
import SupportAgent from './SupportAgent/Agent';

class App extends Component {

  
  constructor() {
    super()
    this.state = { question: '', searchResults: [] }
  }

  render() {
    return (
      <div className="Main">
        <div className="Main-Header">
          <Stack direction="row" spacing={10} justifyContent="space-between" sx={{paddingRight:10 ,paddingLeft:15, paddingTop:2}}>
         
          <Stack spacing={0} >
           <Typography variant="h4" component="h1" gutterBottom>
            Azure Support Agent Demo
           </Typography>
          <AzureFreeAccount/>
          </Stack>
          <Box>
          <img
                src={require('./images/header_img.jpg')}
                height={'210px'}
                style={{ display: 'block' }}
              />
              </Box>
          </Stack>
          
        </div>
        
        <div className="Main-Body">
        
        
          <SupportAgent/>
        
        </div>
        <div className="Main-ServiceCards">
        <AzureServiceCards/>
        </div>
        <div className="Main-Disclaimer">
          <b>Disclaimer: Sample Application</b>
          <br />
          Please note that this sample application is provided for demonstration
          purposes only and should not be used in production environments
          without proper validation and testing.
        </div>
      </div>
    )
  }
}

export default App