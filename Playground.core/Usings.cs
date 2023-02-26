global using System;
global using System.Collections.Generic;
global using System.Collections.Immutable;
global using System.Diagnostics;
global using System.Linq;
global using System.Linq.Expressions;
global using System.Threading.Tasks;
global using System.Reactive.Linq;
global using System.Security.Claims;

global using Microsoft.AspNetCore;
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Authorization.Infrastructure;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.OData.Deltas;
global using Microsoft.AspNetCore.OData.Query;
global using Microsoft.AspNetCore.OData.Routing.Controllers;
global using Microsoft.AspNetCore.SignalR;
global using Microsoft.Extensions.Options;
global using Microsoft.Extensions.Logging;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.OData.ModelBuilder;
global using Microsoft.IdentityModel.Tokens;

global using AutoMapper;
global using AutoMapper.QueryableExtensions;

global using OpenIddict.Abstractions;
global using OpenIddict.Server.AspNetCore;
global using static OpenIddict.Abstractions.OpenIddictConstants;

global using Playground.core;
global using Playground.core.Extensions;
global using Playground.core.Models;
global using Playground.core.Odata;
global using Playground.core.Services;

global using System.Threading;
global using System.Threading.Channels;

global using Swashbuckle.AspNetCore.SwaggerGen;
global using Microsoft.OpenApi.Models;



