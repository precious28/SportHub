#!/usr/bin/env python3
"""
Bedrock AgentCore Harness Service
Provides HTTP endpoint for invoking the SportHub Bedrock Harness
"""

import boto3
import json
import sys
from flask import Flask, request, jsonify, Response
import logging

app = Flask(__name__)
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Initialize Bedrock client
bedrock_client = boto3.client('bedrock-agentcore', region_name='eu-west-1')

# Your Harness ARN
HARNESS_ARN = 'arn:aws:bedrock-agentcore:eu-west-1:262683589693:harness/amusema_sports_agent-Azjer58UxL'

@app.route('/health', methods=['GET'])
def health():
    """Health check endpoint"""
    return jsonify({"status": "healthy", "service": "bedrock-harness-proxy"})

@app.route('/invoke', methods=['POST'])
def invoke_harness():
    """
    Invoke Bedrock AgentCore Harness
    
    Expected JSON body:
    {
        "message": "user message",
        "sessionId": "unique session identifier"
    }
    """
    try:
        data = request.get_json()
        
        if not data or 'message' not in data:
            return jsonify({"error": "Missing 'message' in request"}), 400
        
        user_message = data['message']
        session_id = data.get('sessionId', 'default-session')
        
        # Ensure session ID is at least 33 characters
        runtime_session_id = f"whatsapp-session-{session_id}"
        if len(runtime_session_id) < 33:
            runtime_session_id = runtime_session_id.ljust(33, '0')
        
        logger.info(f"Invoking Harness - Session: {runtime_session_id}, Message: {user_message[:50]}...")
        
        # Invoke the Harness
        response = bedrock_client.invoke_harness(
            harnessArn=HARNESS_ARN,
            runtimeSessionId=runtime_session_id,
            messages=[
                {
                    'role': 'user',
                    'content': [{'text': user_message}]
                }
            ]
        )
        
        # Collect streamed response
        full_response = []
        for event in response['stream']:
            if 'contentBlockDelta' in event:
                delta = event['contentBlockDelta'].get('delta', {})
                if 'text' in delta:
                    full_response.append(delta['text'])
        
        result = ''.join(full_response)
        
        if not result:
            result = "I'm here to help! What would you like to know?"
        
        logger.info(f"Harness response: {len(result)} characters")
        
        return jsonify({
            "response": result,
            "sessionId": runtime_session_id
        })
    
    except Exception as e:
        logger.error(f"Error invoking Harness: {str(e)}")
        return jsonify({
            "error": str(e),
            "response": "I'm having trouble right now. Please try again."
        }), 500

if __name__ == '__main__':
    port = int(sys.argv[1]) if len(sys.argv) > 1 else 5001
    logger.info(f"Starting Bedrock Harness Service on port {port}")
    app.run(host='0.0.0.0', port=port)
