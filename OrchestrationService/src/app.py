from flask import Flask

app = Flask(__name__, )


@app.route('/')
def index():
    app.logger.info('Index')
    return 'Orchestration Service'


if __name__ == '__main__':
    app.run()
    app.logger.info('__main__')
