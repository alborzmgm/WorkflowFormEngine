# WorkflowFormEngine — v6 (.NET 10 + Blazor Server + Bootstrap 5)

## What's New in v6 — Declarative Step Metadata, Textarea Field & Correct List Conditions

| Area | Change |
|---|---|
| **`StepDefinition`** | Added `Description` and `Icon` properties — step card header is now fully data-driven from JSON |
| **`location-workflow.json`** | Each step now declares `"description"` and `"icon"` (Bootstrap Icons class); no UI hardcoding |
| **`WorkflowPage.razor`** | Removed hardcoded `StepIcon` / `StepSubtitle` switch expressions; now reads `CurrentStep.Icon` and `CurrentStep.Description` |
| **`TextareaField.razor`** | New field type — `<textarea>` supporting `rows`, `placeholder`, validation errors |
| **`FieldDefinition`** | Added `Rows` property (default `4`) for `textarea` sizing; ignored by all other field types |
| **`DynamicForm.Resolve()`** | Added `"textarea" => TextareaField` |
| **`location-workflow.json`** | `AdditionalNotes` changed from `"text"` to `"textarea"` with `"rows": 4` |
| **`ConditionEvaluator`** | Fixed: `hasValue`, `isEmpty`, and `contains` now handle `List<string>` explicitly instead of relying on `.ToString()` side-effects |

---

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

## Declarative Step Metadata

Each step in the workflow JSON now carries its own UI presentation metadata:

```json
{
  "stepKey": "location",
  "title": "Location",
  "description": "Where are you located?",
  "icon": "bi-geo-alt-fill",
  "fields": [ ... ]
}
```

`WorkflowPage.razor` reads `CurrentStep.Icon` and `CurrentStep.Description` directly — there is no switch expression or index-based mapping in the UI code. Adding or reordering steps requires only JSON changes.

---

## ConditionEvaluator — Correct List Handling

The evaluator now explicitly branches on `List<string>` values instead of relying on `.ToString()` side-effects:

| Operator | Scalar behaviour | List behaviour |
|---|---|---|
| `hasValue` | field is non-whitespace | list.Count > 0 |
| `isEmpty` | field is whitespace/null | list.Count == 0 |
| `contains` | string.Contains(target) | list.Any(v == target) |
| `equals` | string equality | always false (scalar concept) |
| `notEquals` | string inequality | always true |

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
| `textarea` | `TextareaField` | `string` |
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
| `equals` | yes | field value == target (scalar only) |
| `notEquals` | yes | field value != target (scalar only) |
| `hasValue` | no | field is non-empty; list-aware |
| `isEmpty` | no | field is null/blank; list-aware |
| `contains` | yes | substring match (scalar) or item match (list) |
| `greaterThan` | yes | numeric > value |
| `lessThan` | yes | numeric < value |

## Project Structure

```
WorkflowFormEngine/
├── WorkflowEngine/Models/     FieldDefinition (+ Rows), StepDefinition (+ Description, Icon),
│                              ValidationRule, ConditionRule, FormContext (List-aware)
├── WorkflowEngine/            DependencyGraph, ConditionEvaluator (list-aware), WorkflowLoader
├── Providers/
│   ├── IDataSourceProvider.cs
│   ├── CountryProvider.cs
│   ├── CityProvider.cs
│   ├── ContactMethodsProvider.cs
│   ├── InterestsProvider.cs
│   ├── PrimaryInterestProvider.cs
│   ├── ContactTimeProvider.cs
│   ├── NotificationFrequencyProvider.cs
│   └── NewsletterTopicsProvider.cs
├── Services/
│   ├── OptionService.cs
│   └── ValidationService.cs
├── Components/
│   ├── App.razor
│   ├── Routes.razor
│   ├── FieldComponentBase.cs
│   ├── DynamicForm.razor           "textarea" added to Resolve()
│   ├── TextField.razor
│   ├── TextareaField.razor         NEW — multi-line text input
│   ├── NumberField.razor
│   ├── SelectField.razor
│   └── CheckboxListField.razor
├── Pages/
│   └── WorkflowPage.razor          step icon/description now from JSON
├── wwwroot/
│   ├── css/workflow-engine.css
│   └── workflows/location-workflow.json   step icons/descriptions + textarea for AdditionalNotes
├── _Imports.razor
├── Program.cs
└── WorkflowFormEngine.csproj       net10.0
```

