import React from 'react';
import { Box, Typography, Grid, Link, Paper } from '@mui/material';

function AzureServiceCards() {
  return (
    <Box sx={{ padding: 4}}>
      <Grid container spacing={4} justifyContent="center">
        {/* First Card */}
        <Grid item xs={12} md={4}>
          <Paper elevation={0} sx={{ padding: 4, textAlign: 'center',backgroundColor:'whitesmoke' }}>
            <Typography variant="h6" fontWeight="bold" gutterBottom>
              Popular services free for 12 months
            </Typography>
            <Link href="#" underline="hover" color="primary" sx={{ fontSize: '16px' }}>
              &#x2193; View all services
            </Link>
          </Paper>
        </Grid>

        {/* Second Card */}
        <Grid item xs={12} md={4} >
          <Paper elevation={0} sx={{ padding: 4, textAlign: 'center',backgroundColor:'whitesmoke' }}>
            <Typography variant="h6" fontWeight="bold" gutterBottom>
              55+ other services free always
            </Typography>
            <Link href="#" underline="hover" color="primary" sx={{ fontSize: '16px' }}>
              &#x2193; View all services
            </Link>
          </Paper>
        </Grid>

        {/* Third Card */}
        <Grid item xs={12} md={4}>
          <Paper elevation={0} sx={{ padding: 4, textAlign: 'center',backgroundColor:'whitesmoke' }}>
            <Typography variant="h6" fontWeight="bold" gutterBottom>
              Start with $200 Azure credit
            </Typography>
            <Link href="#" underline="hover" color="primary" sx={{ fontSize: '16px' }}>
              &#x2193; You'll have 30 days to use it—in addition to free services
            </Link>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
}

export default AzureServiceCards;
