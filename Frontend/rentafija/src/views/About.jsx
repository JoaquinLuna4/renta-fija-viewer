import React, { useState } from "react";
import {
	Container,
	Typography,
	Box,
	Link,
	Paper,
	Divider,
	Accordion,
	AccordionSummary,
	AccordionDetails,
} from "@mui/material";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import { styled } from "@mui/material/styles";

// Custom styled Accordion component without borders
const CleanAccordion = styled(Accordion)(({ theme }) => ({
	boxShadow: "none",
	"&:before": {
		display: "none",
	},
	"&.Mui-expanded": {
		margin: 0,
	},
	margin: "8px 0",
	borderRadius: "8px",
	backgroundColor: theme.palette.background.default,
	color: theme.palette.primary.contrastText,
	"& .MuiTypography-root": {
		color: "inherit",
	},
}));

const About = () => {
	const [expanded, setExpanded] = useState(false);

	const handleChange = (panel) => (event, isExpanded) => {
		setExpanded(isExpanded ? panel : false);
	};

	const faqItems = [
		{
			question: "¿Qué es Rentapp?",
			answer:
				"Rentapp es una herramientaque te permite acceder de forma rápida y sencilla a los datos de informes financieros sobre Letras del Tesoro y Cauciones del mercado argentino. En lugar de buscar y analizar manualmente los informes PDF, esta aplicación te presenta la información de forma estructurada, filtrable y fácil de entender.",
		},
		{
			question: "¿De dónde provienen los datos?",
			answer: (
				<>
					Los datos que ves aquí son extraídos directamente de los informes
					públicos del Instituto Argentino de Mercado de Capitales (IAMC).
					Puedes verificar la fuente oficial en su sitio web:{" "}
					<Link
						href="https://www.iamc.com.ar/"
						target="_blank"
						rel="noopener noreferrer"
					>
						www.iamc.com.ar
					</Link>
				</>
			),
		},
		{
			question: "¿Por qué la primera carga de la app es un poco más lenta?",
			answer:
				"El backend de la aplicación está alojado en Render, un servicio que 'duerme' los servidores inactivos para ahorrar costos. La primera vez que accedes después de un período de inactividad, el servidor necesita unos segundos para 'despertarse'. Una vez que está activo, el resto de las solicitudes son mucho más rápidas.",
		},
		{
			question: "¿Con qué frecuencia se actualizan los datos?",
			answer:
				"La aplicación verifica la existencia de un nuevo informe diariamente. Los datos se actualizan tan pronto como el IAMC publica un nuevo informe. Sin embargo, ten en cuenta que los valores reflejados en el informe pueden tener un ligero desfase con respecto al mercado en tiempo real.",
		},
		{
			question: "¿Puedo buscar un activo en particular?",
			answer:
				"¡Claro! La app cuenta con un campo de búsqueda y filtros. Puedes usarlo para encontrar rápidamente activos por su nombre de especie (ticker) o por cualquier otra característica de la tabla.",
		},
		{
			question: "¿Qué tecnologías se usaron para crear esta app?",
			answer: (
				<>
					Esta aplicación es un ejemplo de cómo varias tecnologías trabajan
					juntas:
					<ul>
						<li>
							El frontend (lo que ves) fue construido con React y Material-UI
							para un diseño moderno e intuitivo.
						</li>
						<li>
							El backend (la lógica detrás de escena) fue desarrollado en .NET,
							el cual se encarga de todo el procesamiento de datos.
						</li>
						<li>
							La IA de Google Gemini juega un papel fundamental al interpretar
							los informes y convertir el texto en datos estructurados.
						</li>
						<li>
							Para la optimización, usamos Docker y Render para el alojamiento
							del backend, y Vercel para el frontend. Los resultados de la API
							de Gemini también se almacenan en caché para que las siguientes
							consultas sean casi instantáneas.
						</li>
					</ul>
				</>
			),
		},
	];

	return (
		<Container
			maxWidth="md"
			sx={{
				py: 4,
				color: (theme) => theme.palette.primary.contrastText,
				"& *": {
					color: "inherit",
				},
			}}
		>
			<Paper
				elevation={0}
				sx={{
					p: 4,
					borderRadius: 2,
					backgroundColor: "#00000061",
					boxShadow: "2px 3px #d16f00",
					color: (theme) => theme.palette.primary.contrastText,
					"& *": {
						color: "inherit",
					},
				}}
			>
				<Typography
					variant="h4"
					component="h1"
					gutterBottom
					sx={{ fontWeight: "bold", mb: 3 }}
				>
					Preguntas Frecuentes
				</Typography>

				<Typography paragraph sx={{ mb: 4 }}>
					Bienvenido a Rentapp. A continuación encontrarás respuestas a las
					preguntas más comunes sobre nuestra plataforma.
				</Typography>

				<Box sx={{ width: "100%" }}>
					{faqItems.map((item, index) => (
						<CleanAccordion
							key={index}
							expanded={expanded === `panel${index}`}
							onChange={handleChange(`panel${index}`)}
						>
							<AccordionSummary
								expandIcon={<ExpandMoreIcon />}
								aria-controls={`panel${index}bh-content`}
								id={`panel${index}bh-header`}
								sx={{
									"&:hover": {
										backgroundColor: "action.hover",
									},
								}}
							>
								<Typography
									sx={{ width: "100%", flexShrink: 0, fontWeight: "medium" }}
								>
									{item.question}
								</Typography>
							</AccordionSummary>
							<AccordionDetails>
								<Typography sx={{ color: "inherit" }}>{item.answer}</Typography>
							</AccordionDetails>
						</CleanAccordion>
					))}
				</Box>

				<Box sx={{ mt: 4, pt: 2 }}>
					<Typography
						variant="body2"
						color="text.primary"
						align="center"
						sx={{ opacity: 0.8 }}
					>
						Si tienes más preguntas o necesitas asistencia, no dudes en
						contactarme.
					</Typography>
				</Box>
			</Paper>
		</Container>
	);
};

export default About;
