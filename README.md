# FIBER Project

## Getting Started

To start working with the FIBER project, follow these steps:

1. Clone the repository
2. Make sure you have .NET SDK installed (version 8.0 or later)
3. Restore the NuGet packages:
   ```bash
   dotnet restore
   ```
4. Start the development server:
   ```bash
   dotnet run
   ```

## Build Process

### CSS Build
The project includes a CSS build process that handles the minimization of CSS files. To build the CSS:

```bash
npm run build:css
```

This command will:
- Process all CSS files
- Minify the CSS for production use
- Generate optimized output files

### Full Build
To perform a complete build of the project:

```bash
dotnet build
```

This will:
- Build all project assets
- Minify CSS and other resources
- Prepare the project for production deployment

## Development

During development, you can use:
```bash
dotnet run
```
This will start the development server with hot-reloading enabled.

## Project Structure

The solution is organized in the following way:
- `src/Template.Web` - Main web application
- `src/Template` - Core business logic and data models
- `src/Template.UnitTests` - Unit tests for the project - not implemented
