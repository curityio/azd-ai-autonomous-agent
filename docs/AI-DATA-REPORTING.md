# AI Data Reporting

The user sends various natural language commands from an internet application, as in the following example:

```text
Give me a markdown report on the last 3 months of stock transactions and the value of my portfolio
```

## Example LLM Output

This report summarizes your stock transactions over the last three months, including detailed transaction records and the current value of your portfolio.

### Transaction Summary

| Date       | Stock        | Transaction Type | Quantity | Unit Price (USD) | Total Value (USD) |
|------------|--------------|------------------|----------|------------------|--------------------|
| 2025-09-21 | Company 1    | Buy              | 300      | 426.54           | 127,962.00         |
| 2025-09-21 | Company 4    | Buy              | 200      | 210.75           | 42,150.00          |
| 2025-12-10 | Company 4    | Sell             | -50      | 165.75           | -8,287.50          |
| 2025-12-10 | Company 1    | Sell             | -75      | 376.54           | -28,241.50         |
| 2026-01-19 | Company 1    | Buy              | 50       | 396.54           | 19,827.00          |
| 2026-01-19 | Company 4    | Buy              | 100      | 188.25           | 18,825.00          |

### Total Transactions in the Last 3 Months

| Total Buys   | Total Sells  | Total Transactions  |
|--------------|--------------|---------------------|
| $189,799.00  | $36,529.00   | $153,270.00         |

### Current Holdings

| Stock        | Current Price (USD) | Quantity Owned | Value (USD)       |
|--------------|---------------------|----------------|--------------------|
| Company 1    | 386.54              | 275            | 106,703.50         |
| Company 4    | 180.75              | 250            | 45,187.50          |

### Total Portfolio Value

**Total Value**: $151,891.00
