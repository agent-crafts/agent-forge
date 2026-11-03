(() => {
 const dialog=document.getElementById("addToolDialog");
 const open=document.getElementById("openAddToolDialog");
 const editOpen=document.getElementById("openEditToolDialog");
 const editContext=document.getElementById("toolEditContext");
 if(!dialog||(!open&&!editOpen))return;
 let editMode=false, originalSchema=null;
 const executorSelect=document.getElementById("newToolExecutorKey");
 const apiUrl=document.getElementById("newToolApiUrl");
 const apiUrlField=document.getElementById("newToolApiUrlField");
 const githubFields=document.getElementById("newToolGitHubFields");
 const githubOwner=document.getElementById("newToolGitHubOwner");
 const githubRepo=document.getElementById("newToolGitHubRepo");
 const githubSchema={type:"object",properties:{issue_number:{type:"integer",minimum:1,description:"Positive GitHub issue number."}},required:["issue_number"],additionalProperties:false};
 const httpParameterNames=()=>[...new Set([...apiUrl.value.matchAll(/\\{([^{}]+)\\}/g)].map(match=>match[1]))];
 const httpSchema=()=>{
  const properties={};
  const names=httpParameterNames();
  for(const name of names){
   const previous=editMode&&originalSchema?.properties?.[name];
   properties[name]=previous?structuredClone(previous):{type:"string"};
  }
  return {type:"object",properties,required:names,additionalProperties:false};
 };
 const renderHttpParameters=()=>{
  if(executorSelect.value!=="http.get_json")return;
  rows.replaceChildren();
  for(const name of httpParameterNames()){
   const row=document.createElement("div");
   row.className="ui-modal-parameter";
   const definition=editMode&&originalSchema?.properties?.[name];
   const type=definition?.type||"string";
   const nameField=document.createElement("input");
   nameField.className="ui-input param-name";
   nameField.value=name;
   nameField.disabled=true;
   const typeField=document.createElement("input");
   typeField.className="ui-input param-type";
   typeField.value=type;
   typeField.disabled=true;
   const requiredLabel=document.createElement("label");
   const requiredField=document.createElement("input");
   requiredField.type="checkbox";
   requiredField.className="param-required";
   requiredField.checked=true;
   requiredField.disabled=true;
   requiredLabel.append(requiredField,document.createTextNode("Required"));
   row.append(nameField,typeField,requiredLabel);
   rows.append(row);
  }
  schema();
 };
 apiUrl.addEventListener("input",renderHttpParameters);
 const lockGithubParameters=()=>{
  const locked=executorSelect.value==="get_github_issue";
  const add=get("addToolParameter");
  add.hidden=true;
  if(executorSelect.value==="http.get_json"){renderHttpParameters();return;}
  if(locked){
   rows.replaceChildren();
   const label=document.createElement("div");
   label.className="text-sm text-muted";
   label.textContent="issue_number · integer · required (minimum: 1). Defined by the GitHub issue executor.";
   rows.append(label);
  }else{
   rows.replaceChildren();
   if(editMode&&originalSchema){
    for(const [name,definition] of Object.entries(originalSchema.properties||{})){
     add.click();
     const row=rows.lastElementChild;
     row.querySelector(".param-name").value=name;
     row.dataset.originalName=name;
     row.querySelector(".param-type").value=definition.type||"string";
     row.querySelector(".param-required").checked=(originalSchema.required||[]).includes(name);
    }
   }
  }
  rows.querySelectorAll(".ui-modal-parameter input, .ui-modal-parameter select").forEach(field=>field.disabled=true);
  schema();
 };
 const updateExecutor=()=>{
  apiUrlField.hidden=executorSelect.value!=="http.get_json";
  githubFields.hidden=executorSelect.value!=="get_github_issue";
  lockGithubParameters();
 };
 executorSelect.addEventListener("change",updateExecutor);
 let previewUrl=null;
 const iconInput=document.getElementById("newToolIconUpload");
 const iconPreview=document.getElementById("newToolIconPreview");
 const iconFallback=document.getElementById("newToolIconFallback");
 const iconSrc=value=>{if(!value)return null;if(value.startsWith("/"))return value;if(value.startsWith("tools/")||value.startsWith("agents/"))return "/api/files/"+value;return value;};
 const showIcon=url=>{if(!iconPreview)return;if(url){iconPreview.hidden=false;iconPreview.removeAttribute("hidden");iconPreview.src=url;iconFallback?.setAttribute("hidden","");}else{iconPreview.hidden=true;iconPreview.removeAttribute("src");iconFallback?.removeAttribute("hidden");}};
 iconInput.addEventListener("change",()=>{if(previewUrl)URL.revokeObjectURL(previewUrl);const file=iconInput.files?.[0];previewUrl=file?URL.createObjectURL(file):null;showIcon(previewUrl);});
 const get=id=>document.getElementById(id);
 const status=get("addToolFeedback");
 const update=()=>{const kind=dialog.querySelector('input[name="toolKind"]:checked').value;dialog.querySelectorAll("[data-tool-panel]").forEach(p=>p.hidden=p.dataset.toolPanel!==kind);saveButton.disabled=kind==="external"&&!editMode;saveButton.title=saveButton.disabled?"MCP tool registration is not available yet.":"";status.textContent="";};
 if(open)open.addEventListener("click",()=>{editMode=false;originalSchema=null;get("toolKindSelector").hidden=false;get("addToolTitle").textContent="Add Tool";get("addToolIntro").textContent="Configure a custom tool or connect an external source.";saveButton.textContent="Save";for(const id of ["newToolName","newToolDisplayName","newToolDescription"]){get(id).value="";get(id).readOnly=false;clearFieldError(get(id));}get("newToolCategory").selectedIndex=0;iconInput.value="";showIcon(null);dialog.querySelectorAll('input[name="toolKind"]').forEach(r=>{r.disabled=false;r.checked=r.value==="custom";});rows.replaceChildren();executorSelect.value="";apiUrl.value="";githubOwner.value="";githubRepo.value="";updateExecutor();status.textContent="";resetExternalDiscovery();update();schema();dialog.showModal();});
 if(editOpen&&editContext)editOpen.addEventListener("click",()=>{
  editMode=true;
  get("toolKindSelector").hidden=true;
  const d=editContext.dataset;
  originalSchema=JSON.parse(d.toolSchema);
  executorSelect.value=d.toolExecutorKey||"";
  let savedConfig={};try{savedConfig=JSON.parse(d.toolConfiguration||"{}");}catch{}
  apiUrl.value=savedConfig.url||"";githubOwner.value=savedConfig.owner||"";githubRepo.value=savedConfig.repo||"";updateExecutor();
  get("addToolTitle").textContent="Edit Tool";
  get("addToolIntro").textContent="Update saved tool configuration.";
  saveButton.textContent="Save Changes";
  get("newToolName").value=d.toolName;
  get("newToolName").readOnly=true;
  get("newToolDisplayName").value=d.toolDisplayName;
  get("newToolDescription").value=d.toolDescription;
  get("newToolCategory").value=d.toolCategory;
  iconInput.value="";showIcon(iconSrc(d.toolImageUrl));
  dialog.querySelectorAll('input[name="toolKind"]').forEach(r=>{r.checked=r.value==="custom";r.disabled=true;});
  update();
  lockGithubParameters();
  dialog.showModal();
 });
 dialog.querySelectorAll("[data-modal-close], [data-tool-close], #closeAddToolDialog, #cancelAddToolDialog").forEach(b=>b.addEventListener("click",()=>dialog.close()));
 dialog.querySelectorAll('input[name="toolKind"]').forEach(r=>r.addEventListener("change",update));
 
 const externalDiscoveryStatus=get("externalDiscoveryStatus");
 const externalDiscoveredPanel=get("externalDiscoveredPanel");
 const externalList=get("externalDiscoveredList");
 const search=get("externalToolSearch");
 const rowsVisible=()=>[...externalList.querySelectorAll("[data-mcp-tool-row]")].filter(row=>!row.hidden);
 const selectedCount=()=>externalList.querySelectorAll('input[type="checkbox"]:checked').length;
 const updateSelection=()=>{
  const all=[...externalList.querySelectorAll("[data-mcp-tool-row]")];
  const visible=rowsVisible();
  const checked=visible.filter(row=>row.querySelector("input").checked).length;
  const selectAll=get("externalSelectAll");
  selectAll.checked=visible.length>0&&checked===visible.length;
  selectAll.indeterminate=checked>0&&checked<visible.length;
  selectAll.disabled=visible.length===0;
  get("externalSearchEmpty").hidden=visible.length!==0||all.length===0;
  get("externalSelectedCount").textContent=selectedCount()+" of "+all.length+" tools selected";
  for(const row of all)row.setAttribute("aria-selected",String(row.querySelector("input").checked));
 };
 const resetExternalDiscovery=()=>{
  externalDiscoveredPanel.hidden=true;
  get("externalDiscoveryProgress").hidden=true;
  get("externalSearchEmpty").hidden=true;
  externalList.replaceChildren();
  search.value="";
  get("externalSelectAll").checked=false;
  get("externalSelectAll").indeterminate=false;
  get("externalSelectAll").disabled=true;
  get("externalSelectedCount").textContent="0 selected";
  externalDiscoveryStatus.textContent="Enter a public HTTPS MCP endpoint to discover tools.";
 };
 get("externalToolEndpoint").addEventListener("input",resetExternalDiscovery);
 search.addEventListener("input",()=>{
  const query=search.value.trim().toLowerCase();
  externalList.querySelectorAll("[data-mcp-tool-row]").forEach(row=>{
   row.hidden=!row.textContent.toLowerCase().includes(query);
  });
  updateSelection();
 });
 externalList.addEventListener("change",updateSelection);
 get("externalSelectAll").addEventListener("change",event=>{
  for(const row of rowsVisible())row.querySelector("input").checked=event.target.checked;
  updateSelection();
 });
 get("discoverExternalTools").addEventListener("click",async event=>{
  event.preventDefault();
  resetExternalDiscovery();
  const endpoint=get("externalToolEndpoint").value.trim();
  try{
   const url=new URL(endpoint);
   if(url.protocol!=="https:"||url.username||url.password||(url.port&&url.port!=="443"))throw Error();
  }catch{externalDiscoveryStatus.textContent="Enter a valid public HTTPS MCP endpoint.";return;}
  const button=get("discoverExternalTools");
  button.disabled=true;
  get("externalDiscoveryProgress").hidden=false;
  externalDiscoveryStatus.textContent="";
  try{
   const token=dialog.querySelector('input[name="__RequestVerificationToken"]')?.value;
   const response=await fetch("/api/tools/mcp/discover",{
    method:"POST",credentials:"same-origin",
    headers:{"Content-Type":"application/json",...(token?{"RequestVerificationToken":token}:{})},
    body:JSON.stringify({endpoint})
   });
   const result=await response.json();
   if(!response.ok)throw new Error(result.error||"Discovery failed.");
   const tools=Array.isArray(result.tools)?result.tools:[];
   for(const tool of tools){
    const row=document.createElement("label");
    row.dataset.mcpToolRow="";
    const checkbox=document.createElement("input");
    checkbox.type="checkbox";
    checkbox.checked=true;
    const content=document.createElement("span");
    const name=document.createElement("span");
    name.textContent=tool.name;
    name.title=tool.name;
    const description=document.createElement("span");
    description.textContent=" — "+(tool.description||"No description provided.");
    content.append(name,description);
    row.append(checkbox,content);
    externalList.append(row);
   }
   get("externalFoundCount").textContent="("+tools.length+")";
   externalDiscoveredPanel.hidden=tools.length===0;
   updateSelection();
   externalDiscoveryStatus.textContent=tools.length?"Tools discovered. Select the definitions you need.":"No tools returned by this server.";
   button.textContent="Discover again";
  }catch(error){
    externalDiscoveryStatus.textContent=error.message||"Discovery failed.";
   button.textContent="Retry";
  }finally{get("externalDiscoveryProgress").hidden=true;button.disabled=false;}
 });

 const rows=get("toolParameterRows");
 const buildEditedSchema=()=>{
  const next=structuredClone(originalSchema);
  const properties={};
  const required=[];
  rows.querySelectorAll(".ui-modal-parameter").forEach(row=>{
   const name=row.querySelector(".param-name").value.trim();
   if(!name)return;
   const old=row.dataset.originalName;
   const source=old&&originalSchema.properties&&originalSchema.properties[old];
   const definition=source?structuredClone(source):{};
   definition.type=row.querySelector(".param-type").value;
   properties[name]=definition;
   if(row.querySelector(".param-required").checked)required.push(name);
  });
  next.properties=properties;
  next.required=required;
  return next;
 };
 const schema=()=>{if(executorSelect.value==="http.get_json"){const value=httpSchema();if(window.AgentForgeJsonViewer)window.AgentForgeJsonViewer.render(get("newToolSchema"),value);else get("newToolSchema").textContent=JSON.stringify(value,null,2);return;}if(executorSelect.value==="get_github_issue"){window.AgentForgeJsonViewer?.render(get("newToolSchema"),githubSchema);if(!window.AgentForgeJsonViewer)get("newToolSchema").textContent=JSON.stringify(githubSchema,null,2);return;}const properties={},required=[];rows.querySelectorAll(".ui-modal-parameter").forEach(r=>{const n=r.querySelector(".param-name").value.trim();if(!n)return;properties[n]={type:r.querySelector(".param-type").value};if(r.querySelector(".param-required").checked)required.push(n);});const value=editMode&&originalSchema?buildEditedSchema():{type:"object",properties,required,additionalProperties:false};if(window.AgentForgeJsonViewer){window.AgentForgeJsonViewer.render(get("newToolSchema"),value);}else{get("newToolSchema").textContent=JSON.stringify(value,null,2);}};
 get("addToolParameter").addEventListener("click",()=>{if(executorSelect.value==="get_github_issue")return;const r=document.createElement("div");r.className="ui-modal-parameter";r.innerHTML='<input class="ui-input param-name" aria-label="Parameter name (required)" required pattern="[a-z][a-z0-9_]*" maxlength="155" placeholder="parameter_name" title="Required parameter name"><select class="ui-select param-type" aria-label="Parameter type"><option>string</option><option>integer</option><option>number</option><option>boolean</option><option>array</option><option>object</option></select><label><input type="checkbox" class="param-required" checked>Required</label><button type="button" class="ui-button param-remove" aria-label="Delete parameter" title="Delete parameter"><svg aria-hidden="true" viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M3 6h18M8 6V4h8v2M5 6l1 14h12l1-14M10 10v7M14 10v7"/></svg></button>';r.querySelector(".param-remove").hidden=true;r.querySelectorAll("input, select").forEach(field=>field.disabled=true);r.addEventListener("input",schema);r.addEventListener("change",schema);rows.append(r);schema();});
 const kindValues={custom:1,external:2};
 const categoryValues={general:1,development:2,integration:3,data:4,automation:5};
 const saveButton=get("previewToolConfiguration");
 let saving=false;
 const fieldError=(field,message)=>{
  field.setAttribute("aria-invalid","true");
  field.classList.add("ui-field-invalid");
  let node=document.getElementById(field.id+"Error");
  if(!node){node=document.createElement("p");node.id=field.id+"Error";node.setAttribute("role","alert");field.insertAdjacentElement("afterend",node);}
  node.className="ui-field-error";
  node.textContent=message;
  const described=field.getAttribute("aria-describedby")||"";
  if(!described.split(" ").includes(node.id))field.setAttribute("aria-describedby",(described+" "+node.id).trim());
 };
 const clearFieldError=field=>{
  field.removeAttribute("aria-invalid");
  field.classList.remove("ui-field-invalid");
  const node=document.getElementById(field.id+"Error");
  if(node){node.textContent="";node.classList.add("hidden");}
 };
 dialog.addEventListener("input",e=>{if(e.target.matches("input,textarea,select"))clearFieldError(e.target);});
 dialog.addEventListener("cancel",e=>{if(saving)e.preventDefault();});
 dialog.querySelectorAll("[data-modal-close], [data-tool-close], #closeAddToolDialog, #cancelAddToolDialog").forEach(button=>button.addEventListener("click",e=>{if(saving){e.preventDefault();e.stopImmediatePropagation();}},true));
 saveButton.type="button";
 saveButton.addEventListener("click",async event=>{
  event.preventDefault();
  event.stopPropagation();
  if(saving)return;
  const kind=dialog.querySelector('input[name="toolKind"]:checked').value;
  if(kind!=="custom"&&!editMode){status.textContent="No tools imported. Server-side discovery and registration are not implemented yet.";return;}
  const name=get("newToolName").value.trim();
  const displayName=get("newToolDisplayName").value.trim();
  const description=get("newToolDescription").value.trim();
  let imageUrl=editMode?editContext.dataset.toolImageUrl||null:null;
  const category=categoryValues[get("newToolCategory").value];
  const errors=[];
  const requiredFields=[
   [get("newToolName"),"Tool name is required."],
   [get("newToolDisplayName"),"Display name is required."],
   [get("newToolDescription"),"Description is required."],
   [get("newToolCategory"),"Category is required."]
  ];
  for(const [field,message] of requiredFields){
   clearFieldError(field);
   if(!field.value.trim())errors.push([field,message]);
   else if(!field.checkValidity())errors.push([field,"Enter a valid value."]);
  }
  if(name&&!/^[a-z][a-z0-9_]*$/.test(name)&&!errors.some(([f])=>f===get("newToolName")))errors.push([get("newToolName"),"Use lowercase snake_case."]);
  if(!category&&!errors.some(([f])=>f===get("newToolCategory")))errors.push([get("newToolCategory"),"Select a valid category."]);
  if(executorSelect.value==="http.get_json"){
   if(httpParameterNames().some(name=>!/^[a-z][a-z0-9_]*$/.test(name)))errors.push([apiUrl,"URL placeholders must use lowercase snake_case names."]);
   try{const url=new URL(apiUrl.value);if(url.protocol!=="https:"||(url.port&&url.port!=="443")||url.username||url.password||!url.hostname.includes(".")||url.hostname==="localhost"||url.hostname.endsWith(".localhost"))throw new Error();}
   catch{errors.push([apiUrl,"Enter a public HTTPS API URL (no credentials or custom ports)."]);}
  }
  if(executorSelect.value==="get_github_issue"){
   const owner=githubOwner.value.trim(),repo=githubRepo.value.trim();
   if(!/^[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,37}[a-zA-Z0-9])?$/.test(owner))errors.push([githubOwner,"Enter a valid GitHub owner."]);
   if(!/^[a-zA-Z0-9_.-]{1,100}$/.test(repo)||repo==="."||repo==="..")errors.push([githubRepo,"Enter a valid GitHub repository."]);
  }
  const parameterNames=new Set();
  for(const row of rows.querySelectorAll(".ui-modal-parameter")){
   const field=row.querySelector(".param-name");
   if(!field.id)field.id="toolParam"+Array.from(rows.children).indexOf(row);
   const value=field.value.trim();
   clearFieldError(field);
   if(!value)errors.push([field,"Parameter name is required."]);
   else if(!/^[a-z][a-z0-9_]*$/.test(value)||!field.checkValidity())errors.push([field,"Use lowercase snake_case."]);
   else if(parameterNames.has(value))errors.push([field,"Parameter names must be unique."]);
   parameterNames.add(value);
  }
  if(errors.length){
   errors.forEach(([field,message])=>fieldError(field,message));
   status.textContent="Correct the highlighted fields.";
   errors[0][0].focus();
   return;
  }
  const properties={},required=[];
  rows.querySelectorAll(".ui-modal-parameter").forEach(row=>{
   const key=row.querySelector(".param-name").value.trim();
   if(!key)return;
   properties[key]={type:row.querySelector(".param-type").value};
   if(row.querySelector(".param-required").checked)required.push(key);
  });
  const executorKey=executorSelect.value||null;
  const configurationJson=JSON.stringify(executorKey==="http.get_json"?{url:apiUrl.value.trim()}:executorKey==="get_github_issue"?{owner:githubOwner.value.trim(),repo:githubRepo.value.trim()}:{});
  const jsonSchema=executorKey==="get_github_issue"?JSON.stringify(githubSchema):executorKey==="http.get_json"?JSON.stringify(httpSchema()):editMode&&originalSchema?JSON.stringify(originalSchema):JSON.stringify({type:"object",properties,required,additionalProperties:false});
  status.textContent="";
  saving=true;
  saveButton.disabled=true;
  saveButton.setAttribute("aria-busy","true");
  const originalContent=saveButton.innerHTML;
  saveButton.innerHTML='<span class="inline-flex items-center gap-2"><span class="inline-block size-4 animate-spin rounded-full border-2 border-current border-t-transparent" aria-hidden="true"></span>Saving…</span>';
  status.textContent="Saving tool...";
  try{
   const iconFile=iconInput.files?.[0];
   if(iconFile){
    if(iconFile.size>2*1024*1024||!["image/png","image/jpeg","image/webp","image/gif"].includes(iconFile.type))throw new Error("Choose PNG, JPG, WebP or GIF under 2 MB.");
    const form=new FormData();form.append("file",iconFile);
    const token=document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    const upload=await fetch("/api/files/tools",{method:"POST",credentials:"same-origin",headers:token?{"RequestVerificationToken":token}:{},body:form});
    const result=await upload.json().catch(()=>null);
    if(!upload.ok)throw new Error(result?.error||"Icon upload failed.");
    imageUrl=result.filename;
   }
   const response=await fetch(editMode?"/api/tools/"+editContext.dataset.toolId:"/api/tools",{
    method:editMode?"PUT":"POST",headers:{"Content-Type":"application/json"},
    body:JSON.stringify({...(editMode?{id:editContext.dataset.toolId}:{}),name,displayName,description,kind:kindValues[kind],category,jsonSchema,imageUrl,executorKey,configurationJson})
   });
   if(!response.ok){
    const contentType=response.headers.get("content-type")||"";
    const error=contentType.includes("json")?await response.json():await response.text();
    const message=typeof error==="string"?error:(error.detail||error.title||JSON.stringify(error.errors||error));
    throw new Error(message||"HTTP "+response.status);
   }
   window.AgentForgeToast?.afterNavigation({type:"success",title:editMode?"Tool updated successfully":"Tool created successfully",message:editMode?"Your changes have been saved.":"Your new tool is ready to use."});
   dialog.close();
   window.location.reload();
  }catch(error){
   status.textContent="Could not save tool: "+(error.message||"Unknown error");
   window.AgentForgeToast?.show({type:"error",title:editMode?"Could not update tool":"Could not create tool",message:error.message||"Please try again."});
  }finally{saving=false;saveButton.disabled=false;saveButton.removeAttribute("aria-busy");saveButton.innerHTML=originalContent;}
 });
 update();schema();
 if(!window.AgentForgeJsonViewer){const script=document.createElement("script");script.src="/js/shared/json-viewer.js";script.onload=schema;document.head.append(script);}
})();