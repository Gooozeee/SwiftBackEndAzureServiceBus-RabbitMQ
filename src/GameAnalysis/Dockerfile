FROM python:3.9
WORKDIR /src
COPY ["GameAnalysis/GameAnalysisRabbitMQClient.py", "GameAnalysis/"]
RUN pip install pika
CMD [ "python", "-u", "GameAnalysis/GameAnalysisRabbitMQClient.py"]
