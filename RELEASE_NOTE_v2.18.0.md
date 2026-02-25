## What's Changed in v2.18.0

- Fixed: Elasticsearch empty .keyword field name for string properties — corrected field name generation when CaseInsensitiveFiltering is enabled. (#311)
- Enhancement: Added `IsValid<T>(out List<string> validationErrors)` overload — validates filter value type compatibility with detailed error messages. (#295)

**Full Changelog:** https://github.com/alirezanet/Gridify/compare/v2.17.2...v2.18.0
