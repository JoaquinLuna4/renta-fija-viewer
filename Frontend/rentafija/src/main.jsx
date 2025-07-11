import { createRoot } from "react-dom/client";
import { ThemeProvider } from "@mui/material/styles";

import lightTheme from "./themes/lightTheme.js";
import "./index.css";
import App from "./App.jsx";

const container = document.getElementById("root");
if (container) {
	const root = createRoot(container);

	root.render(
		<ThemeProvider theme={lightTheme}>
			<App />
		</ThemeProvider>
	);
} else {
	throw new Error(
		"Root element with ID 'root' was not found in the document. Ensure there is a corresponding HTML element with the ID 'root' in your HTML file."
	);
}
