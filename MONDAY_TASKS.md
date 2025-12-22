# Monday Tasks - December 23, 2025

## ✅ Deployment Issue Fixed (December 22)

**Railway deployment failing consistently** - RESOLVED
- ✅ Replaced manual Railway CLI installation with official Railway GitHub Action
- ✅ Updated both staging-api.yml and staging-ui.yml workflows
- Changed from `npm install -g @railway/cli@latest` → `railwayapp/railway-deploy@v1`
- This is more reliable and maintained by Railway team
- **Next**: Test deployment on next push to develop

## ✅ Issues Fixed (December 22)

### Tour Behavior Issues - FIXED
- **Skipping tours in Results tab doesn't skip in Crosstab or Stat Sig**
  - ✅ All tour composables already call `skipAllTours()` on cancel
  - ✅ `hasTourBeenCompleted()` checks both individual completion AND skip-all flag
  - ✅ Updated `resetTour()` to clear skip-all flag when restarting tours
  - Solution: When any tour is cancelled, `skipAllTours()` sets skip-all flag → all tours report as completed → tours won't show again

### IV Synchronization Issues - FIXED
- **Changing IV in Crosstab doesn't update in Stat Sig**
  - ✅ Fixed `crosstabSecondQuestion` watcher to always update `route.query.iv` regardless of current tab
  - ✅ Removed condition that only synced when already on statsig tab
  - Solution: IV changes now sync to route immediately, StatSig reads from `route.query.iv` and highlights correct row
  - Files modified:
    - `porpoise-ui/src/views/AnalyticsView.vue` (line 852-869)

## Testing Checklist

Test these scenarios to verify fixes:

### Tour Skip Behavior
- [ ] Start Results tour, click Cancel/Skip → should prevent Crosstab and StatSig tours from showing
- [ ] Start Crosstab tour, click Cancel/Skip → should prevent Results and StatSig tours from showing  
- [ ] Start StatSig tour, click Cancel/Skip → should prevent Results and Crosstab tours from showing
- [ ] Complete Results tour (don't skip) → Crosstab and StatSig tours should still show
- [ ] Click "Restart Tour" button → should clear skip-all flag and allow tour to run again

### IV Synchronization
- [ ] Select IV in Crosstab → switch to StatSig → verify corresponding row is highlighted
- [ ] Change IV in Crosstab → switch to StatSig → verify new row is highlighted
- [ ] Click row in StatSig → switch to Crosstab → verify IV is selected
- [ ] Clear IV in Crosstab → verify `route.query.iv` is removed

## Completed Features
- ✅ Contextual hints system
- ✅ Chart visual consistency (rounded bars, positioned values)
- ✅ Tour manager with skip-all flag
- ✅ Bidirectional IV synchronization
- ✅ Tour reset clears skip-all flag
