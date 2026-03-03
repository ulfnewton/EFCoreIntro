# INSTRUKTIONER – Arbete med EFCoreIntro

*(Elevversion)*

Du ska idag arbeta med projektet:

**<https://github.com/ulfnewton/efcoreintro.git>**

Allt du gör sker i:

**`EFCoreIntro/Program.cs`**

Du ska **inte** ändra i modeller eller DbContext.

***

# Kort om datat (viktigt att veta innan du börjar)

När projektet körs första gången byggs databasen automatiskt och fylls med **Bogus‑genererad testdata** via `SeedAsync()`—detta innebär att du får:

*   minst **8 användare** (en superaktiv, en tyst)
*   minst **4 kanaler** (minst en tom)
*   minst **12 medlemskap**
*   minst **40 meddelanden** över **minst 3 dagar**
*   vissa meddelanden har **identiska timestamps** → viktigt för sortering [\[comeritab-...epoint.com\]](https://comeritab-my.sharepoint.com/personal/ulf_bourelius_comerit_se/Documents/Microsoft%20Copilot%20Chat%20Files/v10.md)

Detta gör att du kan ställa **intressanta frågor** mot datat utan att skapa något själv.

Starta projektet så här:

```bash
dotnet run --project EFCoreIntro/EFCoreIntro.csproj
```

***

# 1. Orientera dig i datat

Skriv ut enklare listor i `Program.cs` för att förstå hur datat ser ut.

Besvara:

1.  Hur många användare finns?
2.  Hur många kanaler, och vilken verkar vara tom?
3.  Vilken kanal verkar ha flest meddelanden?
4.  Kan du hitta minst en användare som *inte* har skickat något meddelande?
5.  Finns det två meddelanden som delar exakt samma `SentAtUtc`?

***

# 2. Frågor om filtrering

### 2.1

Hämta alla kanaler som har **minst två medlemmar**.  
Visa namnet på kanalen och medlemsantalet.

Reflektion: *Hur kan du vara säker på att medlemsantalet stämmer?*

### 2.2

Hämta alla meddelanden som skickats **de senaste 24 timmarna**.  
Visa kanalnamn och text.

Reflektion: *Hur väljer du rätt tidsintervall?*

***

# 3. Frågor om sortering

### 3.1

Hämta alla kanaler och sortera dem efter **senaste meddelande** (descending).

Besvara:

*   Varför kan två kanaler få samma timestamp?
*   Vad händer om du inte lägger till en sekundär sortering?

### 3.2

Förbättra frågan ovan genom att lägga till en **sekundär sortering på kanalnamn**.

Visa de 5 första raderna.

***

# 4. Frågor om projektion

### 4.1

Skapa en projektion med:

*   Kanalnamn
*   Antal medlemmar
*   Text på senaste meddelandet (korta gärna ner till 30 tecken)
*   Tidpunkt för senaste meddelande

Visa de 10 första raderna.

Reflektion: *Varför är det bättre att projicera än att returnera hela entiteter?*

***

# 5. Frågor om aggregation (räkna saker)

### 5.1

Räkna antal meddelanden **per kanal**.  
Sortera så de mest aktiva kanalerna kommer först.

Fråga: *Var resultatet som du förväntade dig? Varför/varför inte?*

### 5.2

Räkna antal meddelanden **per dag**.  
Visa i kronologisk ordning.

Fråga: *Finns det variation mellan dagarna? Varför tror du det ser ut så?*

***

# 6. Frågor om paging

Använd valfri sidstorlek (t.ex. 5 poster per sida).

### 6.1

Visa **sida 1** av en sorterad kanal‑lista.  
Visa även **sida 2**.

Besvara:

*   Vilket indexintervall visar sida 1 och 2?
*   Vad händer om du byter ordning på sortering och paging?

***

# 7. Frågor om `AsNoTracking`

### 7.1

Identifiera två olika queries där du endast *läser* data.  
Lägg till `.AsNoTracking()`.

Skriv vid varje query:

> ”Ingen tracking behövs här eftersom …”

***

# 8. Utmaningsfrågor (fördjupning)

Välj **minst två** av följande:

### 8.1

Hämta de **tre mest aktiva användarna**, mätt i antal skickade meddelanden.

### 8.2

Hitta alla kanaler där **senaste meddelandet** är äldre än 48 timmar.  
Sortera dem alfabetiskt.

### 8.3

Hitta alla användare som är med i **fler än en kanal**.

### 8.4

Skapa en lista som visar:  
`{ ChannelName, MemberCount, MessageCount, LatestMessageText }`  
Sortera först efter MessageCount, sedan efter ChannelName.

***

# 9. Avslutande reflektion

Skriv 6–8 meningar där du besvarar:

*   Vilken fråga var mest utmanande att formulera?
*   Vilken del av datat överraskade dig?
*   Hur påverkade seedningen (Bogus) dina resultat?
*   Vilka frågor mot datat skulle vara relevanta i ett riktigt system?
*   Vad lärde du dig om hur datats form påverkar dina queries?

***

# KLART

Du har nu tränat på att:

*   formulera egna frågor
*   resonera kring datastruktur
*   testa flera sätt att skriva queries
*   analysera resultat
*   motivera dina val

Allt med den kod och data som redan finns i projektet.
