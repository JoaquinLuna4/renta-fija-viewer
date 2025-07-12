function formatDate(dateString) {
	if (!dateString) return ""; // Maneja casos donde la cadena de fecha es nula o vacía

	const date = new Date(dateString);

	// Obtener día, mes y año
	const day = date.getDate();
	const month = date.getMonth() + 1; // getMonth() devuelve 0-11, así que sumamos 1
	const year = date.getFullYear();

	// Formatear día y mes para que siempre tengan dos dígitos (ej. 07 en lugar de 7)
	const formattedDay = String(day).padStart(2, "0");
	const formattedMonth = String(month).padStart(2, "0");

	// Obtener los últimos dos dígitos del año
	const formattedYear = String(year).slice(-2);

	return `${formattedDay}/${formattedMonth}/${formattedYear}`;
}

export default formatDate;
