# SuperInvestor

SuperInvestor is a Blazor application for managing and analyzing investment filings.

## Getting Started

### Prerequisites

- .NET 7.0 SDK or later
- Node.js and npm

### Setting up the development environment

1. Clone the repository
2. Navigate to the project root directory
3. Enter the secrets for appsettings.json

### Installing JavaScript dependencies

1. Ensure you have Node.js and npm installed on your system.
2. In the project root directory, run:

```
npm install
```

This will install all the JavaScript dependencies defined in the `package.json` file.

3. To automatically rebuild the bundle on each change, run:

```
npm run watch
```

This command will watch for changes in your JavaScript files and automatically rebuild the bundle.js file.

### Adding new JavaScript libraries

1. To add a new JavaScript library, use the following command:

```
npm install <library-name> --save
```

For example, to add the `lodash` library:

```
npm install lodash --save
```

2. After installing, you can import and use the library in your JavaScript files:

```javascript
import _ from 'lodash';
```

### Using JavaScript libraries in the project

1. In your JavaScript files (e.g., in the `wwwroot/javascript` directory), you can import and use the installed libraries:

```javascript
import _ from 'lodash';
import Mark from 'mark.js/dist/mark.es6.min.js';

// Use the libraries in your code
```

2. After modifying your JavaScript files, run the build script to bundle your JavaScript:

```
npm run build
```

3. The bundled JavaScript file (`bundle.js`) is already included in your `App.razor` file:

```html
<script src="dist/bundle.js"></script>
```

### Running the application

1. To run the application, use the following command in the project root directory:

```
dotnet run
```

2. Open a web browser and navigate to `https://localhost:7083` (or the port specified in the console output).

## Project Structure

- `wwwroot/javascript`: Contains all JavaScript files for the project
- `Components`: Contains Blazor components
- `Services`: Contains C# service classes
- `wwwroot/dist`: Contains the bundled JavaScript file

## Additional Information

For more detailed information about specific components or services, please refer to the comments in the respective files.