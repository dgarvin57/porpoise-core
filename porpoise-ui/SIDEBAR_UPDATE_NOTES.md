# Sidebar Button Update Pattern

Each button needs these updates for hover tooltips:

1. **Update transition class**: `transition-colors` → `transition-all`
2. **Add group classes**: `sidebarCollapsed ? 'justify-center' : 'space-x-3'` → `sidebarCollapsed ? 'justify-center group relative' : 'space-x-3'`
3. **Remove title attribute**: Delete `:title="sidebarCollapsed ? 'ButtonName' : ''"`
4. **Update SVG classes**: Change from `class="w-5 h-5 flex-shrink-0"` to `:class="['flex-shrink-0 transition-transform', sidebarCollapsed ? 'w-5 h-5 group-hover:scale-110' : 'w-5 h-5']"`
5. **Add tooltip span** after the label:
```vue
<span v-if="sidebarCollapsed" class="absolute left-full ml-2 px-2 py-1 bg-gray-900 dark:bg-gray-700 text-white text-xs rounded opacity-0 group-hover:opacity-100 transition-opacity whitespace-nowrap pointer-events-none z-50">
  ButtonName
</span>
```

Buttons completed:
- ✅ Results
- ✅ Crosstab  
- ✅ Stat Sig

Buttons remaining:
- Full Block
- Matching Blocks
- Index
- Index +
- Profile
- One Response
- Questions
- Data View
- Data Cleansing
- Back to Projects
