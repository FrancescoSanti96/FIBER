# FIBER Project

## Descrizione dell'app

Questa applicazione web consente la **gestione e pubblicazione di bollettini fitosanitari** destinati ad agricoltori e tecnici.

- **Agricoltori**: possono consultare i bollettini disponibili per zona e coltura e scaricare i PDF aggiornati.
- **Tecnici autorizzati**: accedono a un'area riservata per creare, modificare e pubblicare bollettini, con supporto a formati HTML e PDF.

L'app garantisce una comunicazione efficace e tempestiva tra esperti e agricoltori, con un'interfaccia semplice e responsive.

## Utilizzo

Per poter navigare il sito è necessario accedere. Per questo le utenze disponibili si trovano in src/Template.Web/SolutionItems/Accessi.txt

## Processo creativo interfaccia

- (User personas)[https://docs.google.com/spreadsheets/d/1sWZ8q3GJARVO8cwjBEAvXAA2qrboUyiYCCeoHIXVNg0/edit?gid=0#gid=0]
- (Storyboard)[https://www.figma.com/board/xhrCsLonvW0UH8i0yNir1o/FIBER--Storyboards?node-id=0-1&p=f&t=JzSaVqZw8bFMorZT-0]
- (Customer Journey)[https://docs.google.com/document/d/1IiFVUiXB2ZVjXqT0PAuTxd5pX4wPDQMQTuM7A-tmAVo/edit?tab=t.0#heading=h.sct0rnuudlvj]
- (Sketch)[https://trello.com/c/54qbxvpG/11-sketch]
- (Mockup)[https://www.figma.com/design/aPurKMiKOyJv935xo4DNpT/Fiber?node-id=5494-1723&p=f&t=VxK1qcu19DGtII69-0]

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
