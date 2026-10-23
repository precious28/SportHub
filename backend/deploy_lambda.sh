#!/bin/bash

# Deploy Bedrock Harness to AWS Lambda
# Run this script from the backend directory

set -e

echo "🚀 Deploying SportHub Bedrock Service to AWS Lambda..."

# Create deployment package directory
echo "📦 Creating deployment package..."
rm -rf lambda_package
mkdir -p lambda_package

# Install dependencies
echo "📥 Installing dependencies..."
pip install -r lambda_requirements.txt -t lambda_package/ --upgrade

# Copy Lambda function
cp lambda_function.py lambda_package/

# Create ZIP file
echo "📦 Creating deployment ZIP..."
cd lambda_package
zip -r ../lambda_deployment.zip . -q
cd ..

echo "✅ Deployment package created: lambda_deployment.zip"
echo ""
echo "📋 Next steps:"
echo "1. Go to AWS Lambda Console: https://console.aws.amazon.com/lambda"
echo "2. Click 'Create function'"
echo "3. Choose 'Author from scratch'"
echo "4. Function name: sporthub-bedrock-service"
echo "5. Runtime: Python 3.12"
echo "6. Architecture: x86_64"
echo "7. Execution role: Create new role (or use existing with Bedrock permissions)"
echo "8. Click 'Create function'"
echo "9. In 'Code' tab, click 'Upload from' > '.zip file'"
echo "10. Upload lambda_deployment.zip"
echo "11. In 'Configuration' > 'General configuration': Set Timeout to 30 seconds"
echo "12. In 'Configuration' > 'Environment variables': (AWS credentials if not using IAM role)"
echo "13. Add API Gateway trigger (HTTP API)"
echo ""
echo "OR use AWS CLI:"
echo "aws lambda create-function --function-name sporthub-bedrock-service \\"
echo "  --runtime python3.12 \\"
echo "  --role arn:aws:iam::262683589693:role/YOUR_LAMBDA_ROLE \\"
echo "  --handler lambda_function.lambda_handler \\"
echo "  --zip-file fileb://lambda_deployment.zip \\"
echo "  --timeout 30 \\"
echo "  --region eu-west-1"
