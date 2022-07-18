all:
	docker compose -f docker-compose.local.yml build && docker compose -f docker-compose.local.yml up -d

start:
	docker compose -f docker-compose.local.yml up -d

stop:
	docker compose -f docker-compose.local.yml stop

down:
	docker compose -f docker-compose.local.yml down

restart:
	docker compose -f docker-compose.local.yml restart

build:
	docker compose -f docker-compose.local.yml build

clean:
	docker compose -f docker-compose.local.yml down

purge:
	docker compose -f docker-compose.local.yml down -v --rmi all --remove-orphans

prod:
	docker compose -f docker-compose.prod.yml pull && docker compose -f docker-compose.prod.yml up -d

prod-down:
	docker compose -f docker-compose.prod.yml down
