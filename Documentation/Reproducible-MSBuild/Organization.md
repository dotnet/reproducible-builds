# Organization

Each technique is categorized by two qualitative dimensions - Recommendation, and Impact. 

## Recommendation

'Recommendation' characterizes how likely a technique is to pay returns. Generally, any technique documented here is so documented because _someone_, _somewhere_ lost a number of work hours to debugging some non-repeatable build. However, some such issues might be specific to a technique or tool the team is using, so others might never run into that issue. 

- Always: a technique that affects some build behavior that most users will utilize. For instance, resolution of C# framework assemblies.
- Sometimes: a technique that affects only some infrequently used feature. This technique can generally be skipped if difficult to adopt and your team doesn't use that feature
- Rarely: a technique that generally doesn't affect users of the msbuild feature it's improving. This technique should generally be skipped unless you face the specific symptoms described in the technique.

## Impact
'Impact' characterizes how much a technique may interfere with your dev team's workflow. 

- Low: a technique that is generally set once and forget. Developers might not realize it's even there.
- Medium: developers working on the repo are likely to notice this technique in use, but find it generally easy to learn.
- High: a technique that negatively impacts developers often enough that many may consider it too much of a hastle to adopt.