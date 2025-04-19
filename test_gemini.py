import requests
import json

# Replace with your API key
API_KEY = "YOUR_NEW_API_KEY_HERE"
API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent"

def test_gemini():
    headers = {
        "Content-Type": "application/json",
        "x-goog-api-key": API_KEY
    }
    
    data = {
        "contents": [
            {
                "parts": [
                    {
                        "text": "Hello, how are you?"
                    }
                ]
            }
        ]
    }
    
    print(f"Sending request to: {API_URL}")
    print(f"Headers: {headers}")
    print(f"Request Body: {json.dumps(data, ensure_ascii=False)}")
    
    response = requests.post(f"{API_URL}?key={API_KEY}", headers=headers, json=data)
    
    print(f"Response Status: {response.status_code}")
    print(f"Response Content: {response.text}")
    
    if response.status_code == 200:
        response_data = response.json()
        text = response_data["candidates"][0]["content"]["parts"][0]["text"]
        print(f"Response: {text}")
    else:
        print(f"Error: {response.status_code} - {response.text}")

if __name__ == "__main__":
    test_gemini() 