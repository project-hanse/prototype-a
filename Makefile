# Local Development Environment
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

# Prod Environment
prod-start:
	docker compose -f docker-compose.prod.yml pull && docker compose -f docker-compose.prod.yml up -d

prod-stop:
	docker compose -f docker-compose.prod.yml stop

prod-down:
	docker compose -f docker-compose.prod.yml down

# VM Environment
vm-start:
	git pull && docker compose -f docker-compose.vm.yml build && docker compose -f docker-compose.vm.yml pull && docker compose -f docker-compose.vm.yml up -d
vm-stop:
	docker compose -f docker-compose.vm.yml stop
vm-down:
	docker compose -f docker-compose.vm.yml down --remove-orphans
