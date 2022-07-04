SET CHARSET UTF8;

-- create databases
CREATE DATABASE IF NOT EXISTS mlflowdb;
CREATE DATABASE IF NOT EXISTS pipelineservicedb;
CREATE DATABASE IF NOT EXISTS hangfiredb;

-- create user for each database and grant privileges

	-- mlflowdb
	CREATE USER IF NOT EXISTS 'mlflowuser'@'%' IDENTIFIED BY 'hdfcLhDASas3vKhy';
	GRANT ALL PRIVILEGES ON mlflowdb.* TO 'mlflowuser'@'%';

	-- pipelineservicedb
	CREATE USER IF NOT EXISTS 'pipelineserviceuser'@'%' IDENTIFIED BY 'I8TnaeQ0ebeXXZ9n';
	GRANT ALL PRIVILEGES ON pipelineservicedb.* TO 'pipelineserviceuser'@'%';
	GRANT ALL PRIVILEGES ON hangfiredb.* TO 'pipelineserviceuser'@'%';

FLUSH PRIVILEGES;
