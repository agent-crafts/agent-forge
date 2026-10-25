const { chromium } = require('playwright');
(async () => {
  const browser = await chromium.launch({ headless: true });
  const base = process.env.SMOKE_BASE_URL || 'http://127.0.0.1:5088';
  try {
    for (const viewport of [{ width: 390, height: 844 }, { width: 1280, height: 800 }]) {
      const page = await browser.newPage({ viewport });
      const errors = [];
      page.on('pageerror', error => errors.push(error.message));
      await page.goto(base + '/chat', { waitUntil: 'load' });
      await page.locator('#chatForm').waitFor();
      if (await page.locator('.shell').count() !== 1) throw new Error('Chat shell duplicated');
      if (await page.locator('#workspaceSidebar').count() !== 1) throw new Error('Sidebar duplicated');
      if (viewport.width < 640) {
        await page.locator('#workspaceMenu').click();
        if (await page.locator('#workspaceMenu').getAttribute('aria-expanded') !== 'true') throw new Error('Mobile menu failed to open');
        await page.keyboard.press('Escape');
        if (await page.locator('#workspaceMenu').getAttribute('aria-expanded') !== 'false') throw new Error('Mobile menu failed to close');
      }
      const beforeTheme = await page.locator('html').getAttribute('data-theme');
      await page.locator('[data-theme-toggle]').click();
      if (await page.locator('html').getAttribute('data-theme') === beforeTheme) throw new Error('Theme did not change');
      if (!['light','dark'].includes(await page.locator('html').getAttribute('data-theme'))) throw new Error('Theme toggle failed');
      await page.locator('#newChat').click();
      if (await page.locator('#prompt').count() !== 1) throw new Error('Chat composer unavailable');
      if (await page.evaluate(() => document.documentElement.scrollWidth > innerWidth + 1)) throw new Error('Horizontal overflow on Chat');
      if (errors.length) throw new Error('Browser JS errors: ' + errors.join('; '));
      await page.close();
    }
    console.log('Chat UI smoke checks passed at mobile and desktop widths.');
  } finally { await browser.close(); }
})().catch(error => { console.error(error); process.exit(1); });
