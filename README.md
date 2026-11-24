# EstraiP7M-Massivo
Estrazione massiva di file firmati digitalmente (.p7m). Il software permette di selezionare la cartella contenente i file .p7m e di estrarli tutti in un solo comando.
# EstraiP7M Massivo

Utility portatile e gratuita per Windows che consente di estrarre in blocco i documenti originali contenuti all'interno di più file firmati digitalmente (`.p7m` / CAdES).

## Funzionalità principali
- **Estrazione massiva:** elabora tutti i file `.p7m` presenti nella cartella selezionata in un solo clic.
- **Supporto firme multiple e Base64:** rimuove ricorsivamente eventuali firme nidificate (es. `.pdf.p7m.p7m`) e decodifica automaticamente file `.p7m` salvati in formato Base64/PEM.
- **Gestione conflitti:** se nella sottocartella `estratti` esistono già file con lo stesso nome, chiede se sovrascriverli oppure salvarli mantenendo gli originali tramite numerazione progressiva (`documento (1).pdf`).
- **Portatile (Zero installazione):** singolo file eseguibile (`.exe`), avviabile anche da chiavetta USB o tramite trascinamento / menu *Invia a* di Windows.
- **100% Locale:** nessuna connessione a Internet, nessun dato inviato all'esterno.

## Utilizzo
1. Scarica `EstraiP7M.exe` dalla sezione **[Releases](../../releases)** a destra.
2. Avvia il programma con doppio clic.
3. Seleziona la cartella contenente i file `.p7m` e clicca su **Avvia Estrazione**.
4. I documenti estratti verranno salvati nella sottocartella `estratti`.

> **Nota al primo avvio (Windows SmartScreen):** poiché l'eseguibile è distribuito gratuitamente senza un certificato di firma digitale commerciale, Windows potrebbe mostrare l'avviso *"PC protetto da Windows"*. Per avviarlo, cliccare su **Ulteriori informazioni** e poi su **Esegui comunque**. L'intero codice sorgente (`EstraiP7M.cs`) è consultabile e compilabile direttamente in questa repository.
