services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Title", Version = "v1" });
    c.AddSecurityDefinition("jwt_auth", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Description = "Please enter 'Bearer' [space] and then your token in the input below.\n\nExample: 'Bearer 12345abcdef'",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "jwt_auth" } }, new string[] {} }
    });
});

services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                // Handle authentication failure
                return Task.CompletedTask;
            },
        };
    });

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
    c.DefaultModelsExpandDepth(-1);
    c.ShowCommonExtensions();
});