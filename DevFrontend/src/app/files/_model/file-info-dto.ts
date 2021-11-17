export interface FileInfoDto {
	fileName: string;
	fileExtension: string;
	lastModified: Date;
	size?: number;
	bucketName: string;
	objectKey: string;
}
