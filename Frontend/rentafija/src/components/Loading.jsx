import { XlviLoader } from "react-awesome-loaders";
const Loading = () => {
	return (
		<>
			<XlviLoader
				boxColors={["#EF4444", "#F59E0B", "#6366F1"]}
				desktopSize={"50em"}
				mobileSize={"20em"}
			/>
		</>
	);
};

export default Loading;
