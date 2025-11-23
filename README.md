<img src="https://sdk.weixin.senparc.com/images/senparc-logo-500.jpg" /> 

# Senparc.CO2NET

[中文|Chinese](README.zh.md)

<!-- [![Build status](https://mysenparc.visualstudio.com/Senparc%20SDK/_apis/build/status/CO2NET/Senparc.CO2NET%20-ASP.NET%20Core-CI-clone)](https://mysenparc.visualstudio.com/Senparc%20SDK/_build/latest?definitionId=11) -->
[![Build status](https://ci.appveyor.com/api/projects/status/uqhyn9i2x5r300dq/branch/master?svg=true)](https://ci.appveyor.com/project/JeffreySu/senparc-co2net/branch/master)
[![NuGet](https://img.shields.io/nuget/dt/Senparc.CO2NET.svg)](https://www.nuget.org/packages/Senparc.CO2NET)
[![license](https://img.shields.io/github/license/JeffreySu/WeiXinMPSDK.svg)](http://www.apache.org/licenses/LICENSE-2.0)


Senparc.CO2NET is a common foundational extension library supporting .NET Framework and .NET Core, including basic helper classes needed for regular development.  
Developers can directly use CO2NET to provide common foundational methods for their projects, avoiding the pain of repeatedly preparing and maintaining common code.  
Senparc.CO2NET is already relied upon as the foundational library for Senparc series products such as [Senparc.Weixin SDK](https://github.com/JeffreySu/WeiXinMPSDK) and [NCF](https://github.com/NeuCharFramework/NCF).  

  
| Module Function | Nuget Name                     | Nuget                                                                                   | Supported .NET Versions                          |  
|-----------------|--------------------------------|-----------------------------------------------------------------------------------------|--------------------------------------------------|  
| CO2NET Base Library | Senparc.CO2NET   | [![Senparc.CO2NET][1.1]][1.2]    [![Senparc.CO2NET][nuget-img-base]][nuget-url-base]  |  ![.NET 3.5][net35Y]    ![.NET 4.0][net40Y]   ![.NET 4.6.2][net462Y]    ![.NET Core 2.0][core20Y]
| APM Module | Senparc.CO2NET.APM   | [![Senparc.CO2NET.APM][2.1]][2.2]    [![Senparc.CO2NET.APM][nuget-img-base-apm]][nuget-url-base-apm]  |  ![.NET 3.5][net35Y]    ![.NET 4.0][net40Y]   ![.NET 4.6.2][net462Y]    ![.NET Core 2.0][core20Y]
| Redis Base Library | Senparc.CO2NET.Cache.Redis   | [![Senparc.CO2NET.Cache.Redis][3.1]][3.2]    [![Senparc.CO2NET.Cache.Redis][nuget-img-base-redis]][nuget-url-base-redis]  |  ![.NET 3.5][net35N]    ![.NET 4.0][net40N]   ![.NET 4.6.2][net462Y]    ![.NET Core 2.0][core20Y]
| Memcached Base Library | Senparc.CO2NET.Cache.Memcached   | [![Senparc.CO2NET.Cache.Memcached][4.1]][4.2]    [![Senparc.CO2NET.Cache.Memcached][nuget-img-base-memcached]][nuget-url-base-memcached]  |  ![.NET 3.5][net35N]    ![.NET 4.0][net40N]   ![.NET 4.6.2][net462Y]    ![.NET Core 2.0][core20Y]
| WebApi (New) | Senparc.CO2NET.WebApi   | [![Senparc.CO2NET.WebApi][5.1]][5.2]    [![Senparc.CO2NET.WebApi][nuget-img-base-memcached]][nuget-url-base-memcached]  |  ![.NET 3.5][net35N]    ![.NET 4.0][net40N]   ![.NET 4.6.2][net462Y]    ![.NET Core 2.0][core20Y]
| MagicObject (New) | Senparc.CO2NET.MagicObject   | [![Senparc.CO2NET.MagicObject][6.1]][6.2]    [![Senparc.CO2NET.MagicObject][nuget-img-base-magic-object]][nuget-url-base-magic-object]  |  ![.NET 3.5][net35N]    ![.NET 4.0][net40N]   ![.NET 4.6.2][net462Y]    ![.NET Core 2.0][core20Y]


![.NET Core 2.0][core20Y] : Supports .NET Standard 2.0+ and .NET Core 2.1/3.1, .NET 5.0/6.0/7.0/8.0/10.0+

> [!NOTE]
> **.NET 10.0 is the recommended version** for new projects. It provides the latest features, performance improvements, and long-term support.  

> [!NOTE]
> CO2NET will gradually stop supporting .NET Framework 4.0 and earlier versions. Because the official update has stopped. 

[1.1]: https://img.shields.io/nuget/v/Senparc.CO2NET.svg?style=flat
[1.2]: https://www.nuget.org/packages/Senparc.CO2NET
[2.1]: https://img.shields.io/nuget/v/Senparc.CO2NET.APM.svg?style=flat
[2.2]: https://www.nuget.org/packages/Senparc.CO2NET.APM
[3.1]: https://img.shields.io/nuget/v/Senparc.CO2NET.Cache.Redis.svg?style=flat
[3.2]: https://www.nuget.org/packages/Senparc.CO2NET.Cache.Redis
[4.1]: https://img.shields.io/nuget/v/Senparc.CO2NET.Cache.Memcached.svg?style=flat
[4.2]: https://www.nuget.org/packages/Senparc.CO2NET.Cache.Memcached
[5.1]: https://img.shields.io/nuget/v/Senparc.CO2NET.WebApi.svg?style=flat
[5.2]: https://www.nuget.org/packages/Senparc.CO2NET.WebApi
[6.1]: https://img.shields.io/nuget/v/Senparc.CO2NET.MagicObject.svg?style=flat
[6.2]: https://www.nuget.org/packages/Senparc.CO2NET.MagicObject
[7.1]: https://img.shields.io/nuget/v/Senparc.CO2NET.AspNet.svg?style=flat
[7.2]: https://www.nuget.org/packages/Senparc.AspNet.MagicObject

[net35Y]: https://img.shields.io/badge/3.5-Y-brightgreen.svg
[net35N]: https://img.shields.io/badge/3.5-N-lightgrey.svg
[net40Y]: https://img.shields.io/badge/4.0-Y-brightgreen.svg
[net40N]: https://img.shields.io/badge/4.0-N-lightgrey.svg
[net40N-]: https://img.shields.io/badge/4.0----lightgrey.svg
[net45Y]: https://img.shields.io/badge/4.5-Y-brightgreen.svg
[net45N]: https://img.shields.io/badge/4.5-N-lightgrey.svg
[net45N-]: https://img.shields.io/badge/4.5----lightgrey.svg
[net462Y]: https://img.shields.io/badge/4.6.2-Y-brightgreen.svg
[net462N]: https://img.shields.io/badge/4.6.2-N-lightgrey.svg
[coreY]: https://img.shields.io/badge/standard2.0-Y-brightgreen.svg
[coreN]: https://img.shields.io/badge/standard2.0-N-lightgrey.svg
[coreN-]: https://img.shields.io/badge/standard2.0----lightgrey.svg
[core20Y]: https://img.shields.io/badge/standard2.0+-Y-brightgreen.svg
[core20N]: https://img.shields.io/badge/standard2.0+-N-lightgrey.svg

[nuget-img-base]: https://img.shields.io/nuget/dt/Senparc.CO2NET.svg
[nuget-url-base]: https://www.nuget.org/packages/Senparc.CO2NET
[nuget-img-base-apm]: https://img.shields.io/nuget/dt/Senparc.CO2NET.APM.svg
[nuget-url-base-apm]: https://www.nuget.org/packages/Senparc.CO2NET.APM
[nuget-img-base-redis]: https://img.shields.io/nuget/dt/Senparc.CO2NET.Cache.Redis.svg
[nuget-url-base-redis]: https://www.nuget.org/packages/Senparc.CO2NET.Cache.Redis
[nuget-img-base-memcached]: https://img.shields.io/nuget/dt/Senparc.CO2NET.Cache.Memcached.svg
[nuget-url-base-memcached]: https://www.nuget.org/packages/Senparc.CO2NET.Cache.Memcached
[nuget-img-base-aspnet]: https://img.shields.io/nuget/dt/Senparc.CO2NET.AspNet.svg
[nuget-url-base-aspnet]: https://www.nuget.org/packages/Senparc.CO2NET.AspNet
[nuget-img-base-webapi]: https://img.shields.io/nuget/dt/Senparc.CO2NET.WebApi.svg
[nuget-url-base-webapi]: https://www.nuget.org/packages/Senparc.CO2NET.WebApi
[nuget-img-base-magic-object]: https://img.shields.io/nuget/dt/Senparc.CO2NET.MagicObject.svg
[nuget-url-base-magic-object]: https://www.nuget.org/packages/Senparc.CO2NET.MagicObject

## How to Install via Nuget?  
* CO2NET Nuget Address: https://www.nuget.org/packages/Senparc.CO2NET  
* Command:
```shell  
PM> Install-Package Senparc.CO2NET  
```

## Stages  
The currently released version is stable and continuously updated. You can follow the project's progress!  
Assembly online documentation (English version in preparation): <a href="http://doc.weixin.senparc.com/html/G_Senparc_CO2NET.htm" target="_blank">http://doc.weixin.senparc.com/html/G_Senparc_CO2NET.htm</a>  
In the future, this project will be equipped with more comprehensive documentation. If you are eager to try it out, you can open the solution file and refer to the unit test project (Senparc.CO2NET.Tests). Each method has a corresponding usage example. One of the complete goals of this project is to achieve nearly 100% code coverage in unit tests.  
