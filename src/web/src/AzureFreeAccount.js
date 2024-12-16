import React from 'react';
import { Box, Typography, Button, Stack } from '@mui/material';

function AzureFreeAccount() {
  return (
    <Box
      sx={{
        backgroundColor: '#EBEBEB',
        padding: '10px',
        textAlign: 'left',
      }}
    >
     
      <Typography variant="h6" component="h1" gutterBottom>
        Build your next great idea in the cloud with an Azure free account
      </Typography>

   
      <Typography  component="p" gutterBottom sx={{ fontSize: '14px' }}>
        Get started with 12 months of free cloud computing services
      </Typography>

      {/* Buttons */}
      <Stack direction="row" spacing={2} mt={2}>
        <Button
          variant="contained"
          size="small"
          sx={{
            backgroundColor: '#107C10',
            '&:hover': { backgroundColor: '#0e6d0e' },
          }}
        >
          Start free
        </Button>
        <Button
          variant="outlined"
          size="small"
          sx={{
            color: '#0078D4',
            borderColor: '#0078D4',
            '&:hover': { borderColor: '#005a9e', color: '#005a9e' },
          }}
        >
          Pay as you go
        </Button>
      </Stack>
    </Box>
  );
}

export default AzureFreeAccount;
