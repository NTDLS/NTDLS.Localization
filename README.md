# NTDLS.Localization

Localization from a SQLite translation database. The English text in the source code is the key, so an untranslated phrase
(or a missing database) simply shows in English.

## The database

A SQLite file, `Translations\Translations.db` next to the program, with one table:

```
Translation
    Id       INTEGER PRIMARY KEY
    Phrase   TEXT NOT NULL UNIQUE COLLATE NOCASE   -- the English phrase as written in the code: the key
    de, fr, ja, zh-Hans, pt-BR, ...  TEXT          -- one column per language, named by its culture code; empty means "not translated yet"
```

Columns are named by the culture code, exactly as .NET writes it (`de`, `zh-Hans`, `pt-BR`). A regional culture uses its language's
column unless it has a column of its own (`de-AT` uses `de`; plain Norwegian `no` uses `nb`). Databases whose columns still have the
older language names ("German", "Chinese simplified") and a key column still called `English` are read too; `LocalizerScan` renames them to codes the next time it runs. Each language is read once, the first
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

## Adding phrases

A scanner that reads `Localize("...")` calls and the designer files, adds the new phrases to the database and translates the empty
cells is `LocalizerScan` in the FinderSeeker repository.
