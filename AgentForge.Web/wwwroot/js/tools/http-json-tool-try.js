(() => {
  const form=document.getElementById("httpJsonTryForm");
  if(!form)return;
  const status=document.getElementById("httpJsonTryStatus");
  const result=document.getElementById("httpJsonTryResult");
  form.addEventListener("submit",async event=>{
    event.preventDefault();
    const button=form.querySelector('button[type="submit"]');
    button.disabled=true;status.textContent="Running...";result.textContent="";
    try{
      const token=form.querySelector('input[name="__RequestVerificationToken"]')?.value;
      const response=await fetch(form.action,{method:"POST",credentials:"same-origin",headers:{"Content-Type":"application/json",...(token?{"RequestVerificationToken":token}:{})},body:JSON.stringify(Object.fromEntries([...new FormData(form).entries()].filter(([key])=>key!=="__RequestVerificationToken").map(([key,value])=>{const field=form.elements.namedItem(key);const type=field?.dataset?.parameterType;return [key,type==="integer"?Number.parseInt(value,10):type==="number"?Number(value):type==="boolean"?value==="true":value];})))});
      const payload=await response.json();
      status.textContent=payload.success?"Success":"Failed (HTTP "+response.status+")";
      result.textContent=JSON.stringify(payload,null,2);
    }catch(error){status.textContent="Request failed";result.textContent=error.message||String(error);}
    finally{button.disabled=false;}
  });
})();
