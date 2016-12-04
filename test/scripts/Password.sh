curl \
--header "Content-type: application/json" \
--request POST \
--data "{ 'length': 6, 'lowerChars': true, 'upperChars': true, 'digits': true, 'symbols': false, 'hint':'no hint' }" \
-D- \
http://localhost:5000/api/Password/
