import{r as a,c as r,a as t,b as s,w as n,F as d,d as e,e as l,o}from"./app.ece2968d.js";import{_ as h}from"./plugin-vue_export-helper.21dcd24c.js";const g={},y=t("h1",{id:"introduction",tabindex:"-1"},[t("a",{class:"header-anchor",href:"#introduction","aria-hidden":"true"},"#"),e(" Introduction")],-1),c=t("p",null,[e("Gridify is a dynamic LINQ library that converts your "),t("em",null,"strings"),e(" to a LINQ query in the easiest way possible with excellent performance. gridify introduces an Easy and optimized way to apply Filtering, Sorting and pagination using text-based data.")],-1),u=t("p",null,"On of the best use cases of this library is Asp-net APIs, When you need to get some string base filtering conditions to filter data or sort it by a field name or apply pagination concepts to your lists and return a pageable, data grid ready information, from any repository or database. Although, we are not limited to Asp.net projects and we can use this library on any .Net projects and on any collections.",-1),x=t("h2",{id:"how-it-works",tabindex:"-1"},[t("a",{class:"header-anchor",href:"#how-it-works","aria-hidden":"true"},"#"),e(" How It Works")],-1),p=t("p",null,"Gridify use a simple string based query language to convert your string expressions to a LINQ expression. also it extends dotnet LINQ to provide an easy way to filter, sort and paginate your data.",-1),_=t("p",null,"There are two ways to use Gridify:",-1),f=e("Using the "),m=e("Extension"),b=e(" methods"),w=e("Using "),N=e("QueryBuilder"),v=l('<h2 id="performance" tabindex="-1"><a class="header-anchor" href="#performance" aria-hidden="true">#</a> Performance</h2><p>Filtering is the most expensive feature in gridify. the following benchmark is comparing filtering in the most known dynamic linq libraries. As you can see, gridify has the closest result to the native linq.</p><table><thead><tr><th>Method</th><th style="text-align:right;">Mean</th><th style="text-align:right;">Error</th><th style="text-align:right;">StdDev</th><th style="text-align:right;">Ratio</th><th style="text-align:right;">Gen 0</th><th style="text-align:right;">Gen 1</th><th style="text-align:right;">Allocated</th></tr></thead><tbody><tr><td>Native LINQ</td><td style="text-align:right;">740.9 us</td><td style="text-align:right;">7.80 us</td><td style="text-align:right;">6.92 us</td><td style="text-align:right;">1.00</td><td style="text-align:right;">5.8594</td><td style="text-align:right;">2.9297</td><td style="text-align:right;">37 KB</td></tr><tr><td><strong>Gridify</strong></td><td style="text-align:right;">762.6 us</td><td style="text-align:right;">10.06 us</td><td style="text-align:right;">9.41 us</td><td style="text-align:right;">1.03</td><td style="text-align:right;">5.8594</td><td style="text-align:right;">2.9297</td><td style="text-align:right;">39 KB</td></tr><tr><td>DynamicLinq</td><td style="text-align:right;">902.1 us</td><td style="text-align:right;">11.56 us</td><td style="text-align:right;">10.81 us</td><td style="text-align:right;">1.22</td><td style="text-align:right;">19.5313</td><td style="text-align:right;">9.7656</td><td style="text-align:right;">122 KB</td></tr><tr><td>Sieve</td><td style="text-align:right;">977.9 us</td><td style="text-align:right;">6.80 us</td><td style="text-align:right;">6.37 us</td><td style="text-align:right;">1.32</td><td style="text-align:right;">7.8125</td><td style="text-align:right;">3.9063</td><td style="text-align:right;">54 KB</td></tr><tr><td>Fop</td><td style="text-align:right;">2,959.8 us</td><td style="text-align:right;">39.11 us</td><td style="text-align:right;">36.58 us</td><td style="text-align:right;">3.99</td><td style="text-align:right;">46.8750</td><td style="text-align:right;">23.4375</td><td style="text-align:right;">306 KB</td></tr></tbody></table><details class="custom-container details"><p>BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19043.1237 (21H1/May2021Update) 11th Gen Intel Core i5-11400F 2.60GHz, 1 CPU, 12 logical and 6 physical cores .NET SDK=5.0.301 [Host] : .NET 5.0.7 (5.0.721.25508), X64 RyuJIT DefaultJob : .NET 5.0.7 (5.0.721.25508), X64 RyuJIT</p></details>',4);function I(k,B){const i=a("RouterLink");return o(),r(d,null,[y,c,u,x,p,_,t("ul",null,[t("li",null,[f,s(i,{to:"/guide/extensions.html"},{default:n(()=>[m]),_:1}),b]),t("li",null,[w,s(i,{to:"/guide/querybuilder.html"},{default:n(()=>[N]),_:1})])]),v],64)}var E=h(g,[["render",I]]);export{E as default};
