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
      const response=await fetch(form.action,{method:"POST",credentials:"same-origin",headers:{"Content-Type":"application/json",...(token?{"RequestVerificationToken":token}:{})},body:"{}"});
      const payload=await response.json();
      status.textContent=payload.success?"Success":"Failed (HTTP "+response.status+")";
      result.textContent=JSON.stringify(payload,null,2);
    }catch(error){status.textContent="Request failed";result.textContent=error.message||String(error);}
    finally{button.disabled=false;}
  });
})();
