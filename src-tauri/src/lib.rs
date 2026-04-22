use std::path::PathBuf;
use std::process::Command;

#[tauri::command]
fn greet(name: &str) -> String {
    let manifest_dir = PathBuf::from(env!("CARGO_MANIFEST_DIR"));
    let runner_project = manifest_dir
        .parent()
        .map(|p| p.join("src-dotnet").join("BackendRunner").join("BackendRunner.csproj"));

    let Some(runner_project) = runner_project else {
        return "Failed to locate C# backend project.".to_string();
    };

    let output = Command::new("dotnet")
        .arg("run")
        .arg("--project")
        .arg(&runner_project)
        .arg("--")
        .arg(name)
        .output();

    match output {
        Ok(result) if result.status.success() => String::from_utf8_lossy(&result.stdout).trim().to_string(),
        Ok(result) => {
            let error = String::from_utf8_lossy(&result.stderr).trim().to_string();
            format!("C# backend error: {error}")
        }
        Err(err) => format!("Failed to start C# backend: {err}"),
    }
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .invoke_handler(tauri::generate_handler![greet])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
