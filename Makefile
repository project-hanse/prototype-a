start:
	docker-compose -f docker-compose.local.yml up -d

stop:
	docker-compose -f docker-compose.local.yml stop

down:
	docker-compose -f docker-compose.local.yml down

restart:
	docker-compose -f docker-compose.local.yml restart

build:
	docker-compose -f docker-compose.local.yml build

purge:
	docker-compose -f docker-compose.local.yml down -v --rmi all --remove-orphans
