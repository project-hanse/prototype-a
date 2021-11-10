start:
	docker-compose -f docker-compose.yml build && docker-compose -f docker-compose.yml up -d

stop:
	docker-compose -f docker-compose.yml stop

down:
	docker-compose -f docker-compose.yml down
	
restart:
	docker-compose -f docker-compose.yml restart

build:
	docker-compose -f docker-compose.yml build

purge:
	docker-compose -f docker-compose.yml down -v --rmi all --remove-orphans
