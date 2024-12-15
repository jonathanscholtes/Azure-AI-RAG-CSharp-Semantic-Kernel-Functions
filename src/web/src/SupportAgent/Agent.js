import React, { useState, useEffect } from 'react'
import { Button, Box, Link, Stack, TextField } from '@mui/material'
import SendIcon from '@mui/icons-material/Send'
import { Dialog, DialogContent } from '@mui/material'
import ChatLayout from './ChatLayout'
import './Agent.css'

export default function SupportAgent() {

  const [session, setSession] = useState('')
  const [chatPrompt, setChatPrompt] = useState(
    'How do I fix Access Denied with Azure Blob Storage?',
  )
  const [message, setMessage] = useState([
    {
      message: 'Hello, how can I assist you today?',
      direction: 'left',
      bg: '#E7FAEC',
    },
  ])

  const handlePrompt = (prompt) => {
    setChatPrompt('')
    setMessage((message) => [
      ...message,
      { message: prompt, direction: 'right', bg: '#E7F4FA' },
    ])

    fetch(process.env.REACT_APP_API_HOST + '/chat', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ input: prompt, sessionid: session }),
    })
      .then((response) => response.json())
      .then((res) => {
        console.log(res);
        
        setMessage((message) => [
          ...message,
          { message: res.resp, direction: 'left', bg: '#E7FAEC' },
        ])
      })
  }

  const handleSession = () => {
    fetch(process.env.REACT_APP_API_HOST + '/session')
      .then((response) => response.json())
      .then((res) => {
        setSession(res.session_id)
      })
  }


  useEffect(() => {
    if (session === '') handleSession()
  }, [])

  return (
  
          <Stack sx={{paddingRight:10, paddingLeft:10}}>
            <Box sx={{ height: '300px' }}>
              <div className="AgentArea">
                <ChatLayout messages={message} />
              </div>
            </Box>
            <Stack direction="row" spacing={0}>
              <TextField
                sx={{ width: '80%' }}
                variant="outlined"
                label="Message"
                helperText="Chat with AI Support Agent"
                defaultValue="How do I fix Access Denied with Azure Blob Storage?"
                value={chatPrompt}
                onChange={(event) => setChatPrompt(event.target.value)}
              ></TextField>
              <Button
                variant="contained"
                endIcon={<SendIcon />}
                sx={{ mb: 3, ml: 3, mt: 1 }}
                onClick={(event) => handlePrompt(chatPrompt)}
              >
                Submit
              </Button>
            </Stack>
          </Stack>
         
  )
}
