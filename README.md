# MulticastStatsAnalyzer

> A lightweight .NET tool for sending and receiving multicast messages, tracking message statistics such as loss, average value, standard deviation, and more.

## 📦 Project Structure

- **Client** – Console application that receives multicast messages, processes statistics, and supports console commands.
- **Server** – Sends JSON-formatted UDP multicast messages at regular intervals.
- **Shared** – Contains shared models, interfaces, and infrastructure components like base background services and config loaders.

---

## 🚀 How It Works

### 📡 Server
- Sends UDP multicast messages containing serialized `QuoteMessage` objects.
- Uses `UdpClient` with configured address, port, buffer size, and interval.
- Automatically retries on failure.

### 🧠 Client
- Subscribes to the multicast group and receives messages.
- Writes messages into a shared `Channel<QuoteMessage>`.
- Tracks:
  - Total received messages
  - Lost messages (based on missing IDs)
  - Mean, median, mode, and standard deviation
- Accepts console commands (e.g. `stats`) to print current metrics.

---

## 📊 Example Output

```bash
Statistics:
- Received: 31225
- Lost: 52
- Mean: 502.25
- Standard Deviation: 134.78
- Median: 499.00
- Mode: 501.50