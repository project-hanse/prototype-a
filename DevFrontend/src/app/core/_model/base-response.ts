export interface BaseResponse {
	success: boolean;
	errors: Array<Error>;
}

export interface Error {
	message: string;
	code: string;
}
