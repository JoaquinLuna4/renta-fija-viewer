function formatDate(dateString) {
	if (!dateString) return ""; // Maneja casos donde la cadena de fecha es nula o vacía

	const date = new Date(dateString);

	// Obtener día, mes y año
	// Para asegurar de que siempre se muestre la fecha del informe
	// (el día que está en la cadena UTC), se usan los métodos `getUTC...`
	// para extraer las partes de la fecha directamente del objeto Date en UTC.
	const day = date.getUTCDate();
	const month = date.getUTCMonth() + 1; // getUTCMonth() es 0-indexado
	const year = date.getUTCFullYear();

	// Formatear día y mes para que siempre tengan dos dígitos (ej. 07 en lugar de 7)
	const formattedDay = String(day).padStart(2, "0");
	const formattedMonth = String(month).padStart(2, "0");

	// Obtener los últimos dos dígitos del año
	const formattedYear = String(year).slice(-2);

	// Formatear a DD/MM/YY
	return `${formattedDay}/${formattedMonth}/${formattedYear}`;
}

export default formatDate;
