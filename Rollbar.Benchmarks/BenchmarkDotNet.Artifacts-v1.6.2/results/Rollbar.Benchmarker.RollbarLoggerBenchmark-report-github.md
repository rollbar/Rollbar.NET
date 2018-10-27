``` ini

BenchmarkDotNet=v0.11.1, OS=Windows 10.0.17134.376 (1803/April2018Update/Redstone4)
Intel Core i7-8550U CPU 1.80GHz (Max: 1.79GHz) (Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=2.1.403
  [Host]   : .NET Core 2.1.5 (CoreCLR 4.6.26919.02, CoreFX 4.6.26919.02), 64bit RyuJIT
  Clr      : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3221.0
  Core     : .NET Core 2.1.5 (CoreCLR 4.6.26919.02, CoreFX 4.6.26919.02), 64bit RyuJIT
  Mono x64 : Mono 5.16.0 (Visual Studio), 64bit 


```
|                        Method |      Job |  Runtime |       Mean |      Error |     StdDev |     Median |        Min |        Max | Scaled | ScaledSD | Rank |
|------------------------------ |--------- |--------- |-----------:|-----------:|-----------:|-----------:|-----------:|-----------:|-------:|---------:|-----:|
|  AsyncCriticalException_Small |      Clr |      Clr |  24.567 us |  0.5623 us |  1.5393 us |  24.488 us |  20.493 us |  28.364 us |   1.60 |     0.12 |    2 |
|  AsyncCriticalException_Small |     Core |     Core |  15.360 us |  0.4156 us |  0.6221 us |  15.198 us |  14.518 us |  17.095 us |   1.00 |     0.00 |    1 |
|  AsyncCriticalException_Small | Mono x64 | Mono x64 |  35.942 us |  0.7137 us |  1.6961 us |  35.589 us |  33.435 us |  39.837 us |   2.34 |     0.14 |    3 |
|                               |          |          |            |            |            |            |            |            |        |          |      |
| AsyncCriticalException_Medium |      Clr |      Clr |  67.623 us |  1.2597 us |  1.1783 us |  67.655 us |  65.951 us |  70.143 us |   1.24 |     0.08 |    2 |
| AsyncCriticalException_Medium |     Core |     Core |  54.932 us |  1.3084 us |  3.8168 us |  53.980 us |  48.611 us |  66.298 us |   1.00 |     0.00 |    1 |
| AsyncCriticalException_Medium | Mono x64 | Mono x64 |  70.908 us |  1.4081 us |  2.9702 us |  70.115 us |  66.056 us |  78.392 us |   1.30 |     0.10 |    3 |
|                               |          |          |            |            |            |            |            |            |        |          |      |
|  AsyncCriticalException_Large |      Clr |      Clr | 821.785 us | 20.7973 us | 17.3667 us | 816.376 us | 808.427 us | 871.180 us |   1.28 |     0.06 |    3 |
|  AsyncCriticalException_Large |     Core |     Core | 643.248 us | 12.8588 us | 30.3096 us | 634.035 us | 606.716 us | 754.053 us |   1.00 |     0.00 |    2 |
|  AsyncCriticalException_Large | Mono x64 | Mono x64 | 598.382 us | 13.5960 us | 25.5365 us | 593.957 us | 555.388 us | 684.466 us |   0.93 |     0.06 |    1 |
|                               |          |          |            |            |            |            |            |            |        |          |      |
|        AsyncInfoMessage_Small |      Clr |      Clr |   9.327 us |  0.7732 us |  2.2183 us |   8.765 us |   5.294 us |  15.545 us |   2.30 |     0.58 |    2 |
|        AsyncInfoMessage_Small |     Core |     Core |   4.089 us |  0.1226 us |  0.3537 us |   4.122 us |   3.304 us |   4.991 us |   1.00 |     0.00 |    1 |
|        AsyncInfoMessage_Small | Mono x64 | Mono x64 |  13.555 us |  1.0163 us |  2.9967 us |  13.529 us |   7.898 us |  20.025 us |   3.34 |     0.80 |    3 |
|                               |          |          |            |            |            |            |            |            |        |          |      |
|       AsyncInfoMessage_Medium |      Clr |      Clr |   9.083 us |  0.2735 us |  0.7847 us |   9.079 us |   7.221 us |  10.947 us |   2.02 |     0.19 |    2 |
|       AsyncInfoMessage_Medium |     Core |     Core |   4.496 us |  0.0900 us |  0.1936 us |   4.460 us |   4.167 us |   4.935 us |   1.00 |     0.00 |    1 |
|       AsyncInfoMessage_Medium | Mono x64 | Mono x64 |  15.383 us |  0.4463 us |  1.3159 us |  15.513 us |  11.768 us |  18.042 us |   3.43 |     0.32 |    3 |
|                               |          |          |            |            |            |            |            |            |        |          |      |
|         AsynInfoMessage_Large |      Clr |      Clr |  12.484 us |  0.6182 us |  1.7536 us |  11.865 us |  10.115 us |  17.764 us |   1.83 |     0.27 |    2 |
|         AsynInfoMessage_Large |     Core |     Core |   6.829 us |  0.1364 us |  0.3080 us |   6.705 us |   6.446 us |   7.728 us |   1.00 |     0.00 |    1 |
|         AsynInfoMessage_Large | Mono x64 | Mono x64 |  41.267 us |  0.8034 us |  1.0446 us |  41.264 us |  39.416 us |  43.603 us |   6.05 |     0.30 |    3 |
