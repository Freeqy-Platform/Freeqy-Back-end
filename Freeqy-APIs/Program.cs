using Freeqy_APIs;

var builder = WebApplication.CreateBuilder(args);

// Add Dependency 
builder.Services.AddDependency(builder);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();