start:
	docker-compose build && docker-compose up -d

stop:
	docker-compose stop

down:
	docker-compose down
	
restart:
	docker-compose restart

build:
	docker-compose build

purge:
	docker-compose down -v --rmi all --remove-orphans
