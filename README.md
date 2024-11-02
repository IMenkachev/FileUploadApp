# File Upload API Documentation
This API provides endpoints to upload XML files, validate them, convert them to JSON format, and save both the original and converted files.

### Getting Started
The following settings are configured in the application configuration file (**FileUploadSettings**):

- TargetDirectory: Directory path for saving uploaded files.
- JsonOutputPath: Directory path for storing JSON-converted files.
- UploadEndpointPath: Path for the upload endpoint.
- KestrelMaxRequestBodySize: Maximum request body size, set to 50 MB for this API.

### Configuration (appsettings.json)
*"FileUploadSettings": {
  "TargetDirectory": "UploadedFiles",
  "JsonOutputPath": "wwwroot/json-output-files",
  "UploadEndpointPath": "/api/FileUpload/upload-multiple",
  "KestrelMaxRequestBodySize": 52428800 // 50 MB
}*

## API Endpoints
### Upload Multiple XML Files
Upload multiple XML files to be saved, validated, converted to JSON, and stored.

**Request**
- **URL:** /api/FileUpload/upload-multiple
- **Method:** POST
- **Content-Type:** multipart/form-data
- **Parameters:**
- - **files:** List of XML files to be uploaded (required). Each file must have a .xml extension.

### Error Handling
The API includes robust error handling to ensure clear feedback:

- **File Validation Errors:** Empty files, non-XML files, or files with invalid XML syntax will be logged as warnings and not prevent successful files from processing.
- **Server Errors:** If an unexpected server error occurs, a 500 status code is returned with a standard error message.
- **File Size Limits:** Requests exceeding 50 MB return a 413 status code.

### Middlewares
This API integrates custom middleware for file validation and error handling:

- **FileValidationMiddleware:** Validates XML file structure and file size, ensuring only valid files proceed to processing.
- **ErrorHandlingMiddleware:** Logs unhandled exceptions and returns a standard 500 response for unexpected errors.
