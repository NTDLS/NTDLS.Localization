# NTDLS.Localization

Localization from a SQLite translation database. The English text in the source code is the key, so an untranslated phrase
(or a missing database) simply shows in English.

## The database

A SQLite file, `Translations\Translations.db` next to the program, with one table:

```
Translation
    TranslationId  INTEGER PRIMARY KEY
    Phrase   TEXT NOT NULL UNIQUE COLLATE NOCASE   -- the English phrase as written in the code: the key
    de, fr, ja, zh-Hans, pt-BR, ...  TEXT          -- one column per language, named by its culture code; empty means "not translated yet"
    LastModified  TEXT                             -- when the row was last changed (UTC, yyyy-MM-dd HH:mm:ss); not used when looking up
```

The row id column is not called `Id` because SQLite column names are not case sensitive and `id` is the code for Indonesian.

Columns are named by the culture code, exactly as .NET writes it (`de`, `zh-Hans`, `pt-BR`). A regional culture uses its language's
column unless it has a column of its own (`de-AT` uses `de`; plain Norwegian `no` uses `nb`). Databases whose columns still have the
older language names ("German", "Chinese simplified") and a key column still called `English` are read too; `LocalizationScan` renames them to codes the next time it runs. Each language is read once, the first
time it is needed, and then served from memory. The file is only ever read, never locked or changed.

Copy it to the output folder from the project:

```xml
<ItemGroup>
  <None Include="Translations\Translations.db" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

## Using it

```csharp
using NTDLS.Localization;

Localizer.Default.SetLanguage("de");          // "de", "pt-BR", or empty / "auto" to follow Windows

Text = Localizer.Default.Localize("Search");
MessageBox.Show(Localizer.Default.Localize("Copied {0} files to {1}", count, folder));
```

Use a format string with `{0}`, `{1}` for text that has values in it (not `$"..."` or `+`), so translators get the whole sentence.
The translation must keep the same placeholders; if it does not, the English text is used.

To write just `Localize("Search")`, add a global using for the static helper to the project:

```xml
<ItemGroup>
  <Using Include="NTDLS.Localization.StaticLocalizer" Static="true" />
</ItemGroup>
```

## Passing it to other libraries: `ILocalizer`

A library that shows text takes an `ILocalizer`, so it does not care how the application stores translations:

```csharp
public class MyLibrary(ILocalizer? localizer = null)
{
    private readonly ILocalizer _localizer = localizer ?? NullLocalizer.Instance;

    public string Describe(int count) => _localizer.Localize("Found {0} items", count);
}

var library = new MyLibrary(Localizer.Default);
```

`NullLocalizer.Instance` returns the English text, for libraries used without a localizer and for tests.

## Windows Forms (the `net10.0-windows` build)

`FormLocalizer` translates the text the designer put on a form: titles, labels, buttons, check boxes, group boxes, tab pages, menus,
list columns and text box placeholders. The designer's text is the phrase, so designer code needs no `Localize(...)` calls.

```csharp
public FormMain()
{
    InitializeComponent();
    FormLocalizer.Apply(this);          // call again after changing the language
}
```

The designer's original text is remembered, so the form is translated from English each time. Text that the code set itself is left
alone, so the form must update that itself.

## Logging

The package writes no log. To see its warnings (a missing database, a broken placeholder), set:

```csharp
LocalizationDiagnostics.OnWarning = (message, exception) => Log.Warning(message, exception);
LocalizationDiagnostics.OnError = (message, exception) => Log.Error(message, exception);
```

## Adding phrases: LocalizationScan

[LocalizationScan](https://github.com/NTDLS/LocalizationScan) is a command line tool that keeps the translation database in step with
your source code. It:

1. scans every `*.cs` file for `Localize("...")` calls (normal and verbatim strings, formatted text, and calls through a localizer;
   calls in comments are ignored) and for the text the Windows Forms designer put on forms, and adds the new phrases to the database
   (creating the database, with a column for every supported language, if it does not exist yet);
2. clears existing translations that are certainly broken, so they are translated again;
3. translates the empty cells, one batch of phrases at a time, with several languages in parallel.

Existing, valid translations are never overwritten, so it is safe to run as often as you like. Phrases that cannot be translated are
left empty and tried again on the next run.

```
LocalizationScan <rootPath> <projectName> [concurrency]
```

| Argument | Meaning |
| --- | --- |
| `rootPath` | Folder to scan, typically the solution folder. |
| `projectName` | Project folder under `rootPath` that holds `Translations\Translations.db`. |
| `concurrency` | Languages to translate at the same time (default 4). |

```
LocalizationScan "C:\Source\MyApp" "MyApp.UI" 8
```

Only literal strings can be scanned: `Localize("Search")` is found, `Localize(someVariable)` and `Localize($"...")` are not.
The `bin`, `obj`, `.git`, `.vs`, `packages`, `Tests`, `Translations` and `publish` folders, and folders ending in `.Tests`, are skipped.

Translations come from an Einkr model and need an API key, given in the `LOCALIZATION_SCAN_API_KEY` environment variable or in a file
(`C:\EinkrKey.txt`, or the file named by `LOCALIZATION_SCAN_KEY_FILE`). `LOCALIZATION_SCAN_MODEL` chooses the model, and
`LOCALIZATION_SCAN_ESCALATION_MODEL` names an optional larger model that is given the phrases the first one fails on.

A translation is rejected, and retried, when it is empty, changes or drops a placeholder (`{0}`, `{1:N0}`), adds markup the English does
not have, or contains letters from the wrong alphabet for the language. This is what keeps a broken translation from replacing the
English text at run time (see [Using it](#using-it)).

The tool needs the .NET 10 runtime. See its repository for the installer and the full documentation.
