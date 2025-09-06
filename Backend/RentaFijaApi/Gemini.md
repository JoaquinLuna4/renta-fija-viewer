Project Context for Gemini CLI
This API is a specialized web scraping tool designed to obtain and process data on financial assets from the IAMC website in Argentina. The project focuses on extracting specific financial information from daily reports on Treasury Bills and Bonds. The core problem it solves is the manual and time-consuming process of retrieving and structuring this data, which is often published in a semi-structured format within PDF files.

Technological Stack
Platform: The API is built using .NET (likely .NET 8), leveraging the C# language for its core logic.

Web Scraping & I/O:

HTTP Client: HttpClient is used to download PDF reports from the IAMC website.

HTML Parsing: The AngleSharp library is used to parse the HTML of web pages to find the URL of the latest report.

PDF Processing: The UglyToad.PdfPig library is used to extract plain text from PDF documents.

API & Data Management:

API Framework: The project uses ASP.NET for its API layer.

Data Models: Data is structured using C# DTOs (Data Transfer Objects) with System.Text.Json.Serialization attributes to map complex JSON output from the AI.

Caching: Results are cached to minimize redundant requests to the IAMC website and the Gemini API, improving performance and efficiency.

Role of the AI (Gemini)
Gemini's primary role is to act as a data interpreter. It receives the raw, unformatted text extracted from the PDF reports. Gemini is given a detailed prompt with the following instructions:

Extract Specific Data: Identify and extract financial data points from the text, such as Especie, Fecha de Emisión, Plazo al Vto, Tasa de licitación, and others as defined in the project's DTOs.

Format the Output: Structure the extracted data into a specific JSON format that conforms to the LetrasTesoroDto data model. This ensures a clean and consistent output for the API.

Handle Ambiguity: The prompt should instruct Gemini to gracefully handle any inconsistencies or formatting issues in the source text, ensuring the output is always a valid JSON object.

Special Instructions for Code Generation
DO NOT remove existing comments from my code if you are providing a corrected or modified version. My comments are important for documenting the logic, debugging, and providing context. Any new code or modifications you propose should be accompanied by clear explanations, but the original comments must be preserved.