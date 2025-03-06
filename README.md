# IoT Operations Sample - IOLink.NET via MQTT

Dieses Repository dient als Beispiel, wie man **IOLink.NET** an den integrierten **IoT Operations MQTT Broker** anbindet.   
Die Anwendung läuft in einer Endlosschleife als **Docker-Container** und veröffentlicht die gelesenen Daten vom **IOLinkPortReader** über MQTT-Nachrichten im definierten Zeitintervall.

## Features
- Verbindung zu einem IO-Link Master mittels **IOLink.NET**
- Datenlesen mit **IOLinkPortReader**
- MQTT Client zur Veröffentlichung der Sensordaten
- Docker-Container für einfache Bereitstellung

## Voraussetzungen
- **.NET 8+**
- **Docker** (zum Bauen und Ausführen des Containers)
- **Ein MQTT Broker** (z. B. Eclipse Mosquitto, HiveMQ, EMQX)
- **IO-Link Master** mit angeschlossenem Sensor

## Installation & Nutzung

### 1. Repository klonen
```sh
git clone https://github.com/domdeger/iot-operations-iolink-drop.git
cd iot-operations-iolink-drop
```

### 2. Konfiguration
Passe die Datei `appsettings.json` an, um den **IO-Link Master** und den **MQTT Broker** zu konfigurieren:

```json
{
  "IOLink": {
    "MasterIP": "192.168.1.100",
    "Port": 1
  },
  "Mqtt": {
    "ClientId": "IOLink-Demo-MQTT-Client",
    "BrokerHost": "your-broker-address",
    "BrokerPort": 1883,
    "PublishIntervalSeconds": 60
  }
}

Konfigurationswerte können durch Umgebungsvariablen nach folgendem Schema überschrieben werden Section__Value. Beispiel: IOLink__MasterIP
```

### 3. Docker-Container bauen und starten

```sh
docker compose up --build -d
```

### 4. Logs überprüfen

```sh
docker compose logs -f iotoperationsdrop.iolink
```

## MQTT Topics
Das Programm sendet periodisch die IO-Link Sensordaten an das konfigurierte MQTT-Topic. Beispiel:

```
topic: iolink/pdin
payload: { ... }
```

## Entwicklung
Falls du den Code lokal ausführen möchtest, installiere die Abhängigkeiten mit:

```sh
dotnet restore
dotnet run
```

## Lizenz
Dieses Projekt steht unter der MIT-Lizenz. Siehe [LICENSE](LICENSE) für weitere Details.

## Autor
**Tim** – [GitHub Profil](https://github.com/tim1993)
**Dominik** – [GitHub Profil](https://github.com/domdeger)

---
**Hinweis:** Dies ist ein Beispiel-Repository für die Integration von IO-Link und MQTT mit .NET.

