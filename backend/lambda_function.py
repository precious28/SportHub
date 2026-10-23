"""
AWS Lambda function for Bedrock AgentCore Harness
Optimized for Lambda + API Gateway integration
"""

import boto3
import json
import logging

# Configure logging
logger = logging.getLogger()
logger.setLevel(logging.INFO)

# Initialize Bedrock client (reused across invocations)
bedrock_client = boto3.client('bedrock-agentcore', region_name='eu-west-1')

# Your Harness ARN
HARNESS_ARN = 'arn:aws:bedrock-agentcore:eu-west-1:262683589693:harness/amusema_sports_agent-Azjer58UxL'

def lambda_handler(event, context):
    """
    Lambda handler for API Gateway proxy integration
    """
    
    # Handle preflight CORS requests
    if event.get('httpMethod') == 'OPTIONS':
        return {
            'statusCode': 200,
            'headers': {
                'Access-Control-Allow-Origin': '*',
                'Access-Control-Allow-Headers': 'Content-Type',
                'Access-Control-Allow-Methods': 'POST, OPTIONS'
            },
            'body': ''
        }
    
    # Handle GET /health
    if event.get('httpMethod') == 'GET' and event.get('path', '').endswith('/health'):
        return {
            'statusCode': 200,
            'headers': {
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*'
            },
            'body': json.dumps({
                "status": "healthy",
                "service": "bedrock-harness-lambda"
            })
        }
    
    try:
        # Parse request body
        body = json.loads(event.get('body', '{}'))
        
        if 'message' not in body:
            return {
                'statusCode': 400,
                'headers': {
                    'Content-Type': 'application/json',
                    'Access-Control-Allow-Origin': '*'
                },
                'body': json.dumps({"error": "Missing 'message' in request"})
            }
        
        user_message = body['message']
        session_id = body.get('sessionId', 'default-session')
        
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
        for event_chunk in response['stream']:
            if 'contentBlockDelta' in event_chunk:
                delta = event_chunk['contentBlockDelta'].get('delta', {})
                if 'text' in delta:
                    full_response.append(delta['text'])
        
        result = ''.join(full_response)
        
        if not result:
            result = "I'm here to help! What would you like to know?"
        
        logger.info(f"Harness response: {len(result)} characters")
        
        return {
            'statusCode': 200,
            'headers': {
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*'
            },
            'body': json.dumps({
                "response": result,
                "sessionId": runtime_session_id
            })
        }
    
    except Exception as e:
        logger.error(f"Error invoking Harness: {str(e)}")
        return {
            'statusCode': 500,
            'headers': {
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*'
            },
            'body': json.dumps({
                "error": str(e),
                "response": "I'm having trouble right now. Please try again."
            })
        }
