# WorkflowFormEngine — v5 (.NET 10 + Blazor Server + Bootstrap 5)

## What's New in v5 — CheckboxList Support

| Area | Change |
|---|---|
| **`CheckboxListField.razor`** | New field type — Bootstrap `form-check` grid, Select All / Clear All, shimmer loading, selected-count badge |
| **`ContactMethodsProvider.cs`** | New data source — `"ContactMethods"` |
| **`InterestsProvider.cs`** | New data source — `"Interests"` (country-dependent, shows DependsOn works on checkbox lists too) |
| **`FormContext.HasValue()`** | Updated to handle `List<string>` — a non-empty list = has value |
| **`ValidationService`** | New `minItems` and `maxItems` rule types for list fields; scalar rules silently pass on list fields and vice-versa |
| **`DynamicForm.Resolve()`** | Added `"checkboxlist" => CheckboxListField` |
| **`location-workflow.json`** | Added `Interests` (checkboxlist, country-dependent, max 4) and `PreferredContactMethods` (checkboxlist, required, min 1, max 3) |
| **`WorkflowPage` summary** | List values render as Bootstrap `badge` pills instead of raw `ToString()` |
| **`workflow-engine.css`** | Added `.checkbox-list-scroll` and `.form-check--selected` |

---

## How the CheckboxList Works

### Value Storage
`CheckboxListField` stores its selections in `FormContext` as `List<string>`.
All other field types store `object?` scalars. Both are transparent to `DynamicForm`
and `WorkflowPage` — they only see `object?`.

```
User checks "Email" + "SMS"
    │
    └── CheckboxListField.PushValue()
            → Context.SetValue("PreferredContactMethods", List<string>{"email","sms"})
            → OnValueChanged fires → DynamicForm.OnFieldChanged()
            → BFS cascade resets dependents (if any)
            → OnContextChanged → WorkflowPage re-renders
```

### Validation
Two new rule types apply specifically to list fields:

| type | value field | description |
|---|---|---|
| `minItems` | int | at least N items selected |
| `maxItems` | int | at most N items selected |

Scalar rules (`minLength`, `regex`, etc.) on a list field are silently skipped.
List rules (`minItems`, `maxItems`) on a scalar field are silently skipped.
This makes mixed-type step validation safe with zero branching in `WorkflowPage`.

### DependsOn on a CheckboxList
Works identically to `SelectField` — the `Interests` checkboxlist depends on `CountryId`.
When `CountryId` changes, `DependencyGraph` resets `Interests`, and `InterestsProvider`
receives the new `FormContext` and returns country-filtered options.

---

## Quick Start

```bash
cd WorkflowFormEngine
dotnet run
```
Navigate to `https://localhost:5001/workflow`

---

## All Supported Field Types

| fieldType | Component | Value type in FormContext |
|---|---|---|
| `text` | `TextField` | `string` |
| `number` | `NumberField` | `decimal` |
| `select` | `SelectField` | `string` |
| `checkboxlist` | `CheckboxListField` | `List<string>` |

## All Supported ValidationRule Types

| type | Applies to | value field | description |
|---|---|---|---|
| `required` | all | — | non-empty / at least one checked |
| `minLength` | scalar | int | string length >= N |
| `maxLength` | scalar | int | string length <= N |
| `min` | number | decimal | value >= N |
| `max` | number | decimal | value <= N |
| `regex` | scalar | — | value matches pattern |
| `email` | scalar | — | valid email format |
| `minItems` | checkboxlist | int | at least N selected |
| `maxItems` | checkboxlist | int | at most N selected |

## All Supported ConditionRule Operators

| operator | value needed? | description |
|---|---|---|
| `equals` | yes | field value == target |
| `notEquals` | yes | field value != target |
| `hasValue` | no | field is non-empty (works with List too) |
| `isEmpty` | no | field is null/blank |
| `contains` | yes | string contains substring |
| `greaterThan` | yes | numeric > value |
| `lessThan` | yes | numeric < value |

## Project Structure

```
WorkflowFormEngine/
├── WorkflowEngine/Models/     FieldDefinition, ValidationRule, ConditionRule, FormContext (List-aware)
├── WorkflowEngine/            DependencyGraph, ConditionEvaluator, WorkflowLoader
├── Providers/
│   ├── IDataSourceProvider.cs
│   ├── CountryProvider.cs
│   ├── CityProvider.cs
│   ├── ContactMethodsProvider.cs   NEW — "ContactMethods" key
│   └── InterestsProvider.cs        NEW — "Interests" key, country-dependent
├── Services/
│   ├── OptionService.cs
│   └── ValidationService.cs        minItems / maxItems added
├── Components/
│   ├── App.razor
│   ├── Routes.razor
│   ├── FieldComponentBase.cs
│   ├── DynamicForm.razor           "checkboxlist" added to Resolve()
│   ├── TextField.razor
│   ├── NumberField.razor
│   ├── SelectField.razor
│   └── CheckboxListField.razor     NEW
├── Pages/
│   └── WorkflowPage.razor          badge rendering for List<string> values
├── wwwroot/
│   ├── css/workflow-engine.css     checkbox-list-scroll + form-check--selected
│   └── workflows/location-workflow.json   Interests + PreferredContactMethods fields added
├── _Imports.razor
├── Program.cs                      ContactMethodsProvider + InterestsProvider registered
└── WorkflowFormEngine.csproj       net10.0
```
