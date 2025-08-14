```mermaid
classDiagram
    class TariffBase
    class CosyTariff
    class DayNightTariff
    class FixedTariff
    class IntervalTariff
    class TariffType

    CosyTariff --|> TariffBase
    DayNightTariff --|> TariffBase
    FixedTariff --|> TariffBase
    IntervalTariff --|> TariffBase
```
